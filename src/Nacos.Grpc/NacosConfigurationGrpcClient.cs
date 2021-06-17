using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nacos.Exceptions;
using Nacos.Grpc.Messages;

namespace Nacos.Grpc
{
    /// <summary>
    /// Nacos Grpc 配置客户端
    /// </summary>
    public sealed class NacosConfigurationGrpcClient : NacosGrpcClient, INacosConfigurationClient
    {
        #region Private 字段

        private readonly Dictionary<INacosUniqueConfiguration, ConfigurationSubscribeState> _notifyCallbacks = new(new INacosUniqueConfigurationEqualityComparer());

        #endregion Private 字段

        #region Public 构造函数

        /// <inheritdoc cref="NacosConfigurationGrpcClient"/>
        public NacosConfigurationGrpcClient(NacosGrpcClientOptions clientOptions) : base(clientOptions)
        {
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public async Task<NacosConfigurationDescriptor> GetConfigurationAsync(NacosConfigurationDescriptor descriptor, CancellationToken token = default)
        {
            CheckInitiated();
            CheckDisposed();

            var request = new ConfigQueryRequest(descriptor);
            request.Headers.Add("notify", "false");

            var response = await RequestAsync<ConfigQueryResponse>(request, token).ConfigureAwait(false);

            if (response.ErrorCode == NacosErrorCode.ConfigurationNotFound)
            {
                throw new ConfigurationNotFoundException(descriptor);
            }

            return descriptor.WithContent(response.Content, response.Md5);
        }

        /// <inheritdoc/>
        public async Task<IAsyncDisposable> SubscribeConfigurationChangeAsync(NacosConfigurationDescriptor descriptor,
                                                                              ConfigurationChangeNotifyCallback notifyCallback,
                                                                              CancellationToken token = default)
        {
            //HACK 实现批量订阅？

            CheckInitiated();
            CheckDisposed();

            await InternalSubscribeConfigurationChangeAsync(descriptor, token).ConfigureAwait(false);

            lock (_notifyCallbacks)
            {
                if (_notifyCallbacks.TryGetValue(descriptor, out var existSubscribeState))
                {
                    existSubscribeState.NotifyCallback += notifyCallback;
                }
                else
                {
                    _notifyCallbacks.Add(descriptor, new(descriptor, notifyCallback));
                }
            }

            return new ConfigurationChangeUnsubscriber(descriptor, notifyCallback, UnSubscribeConfigurationChange);
        }

        #endregion Public 方法

        #region Protected 方法

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            lock (_notifyCallbacks)
            {
                _notifyCallbacks.Clear();
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void OnConnectionRestore(CancellationToken token)
        {
            Logger?.LogInformation("连接已恢复");

            ConfigurationSubscribeState[] subscribeStates;

            lock (_notifyCallbacks)
            {
                subscribeStates = _notifyCallbacks.Values.ToArray();
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    var tasks = subscribeStates.Select(subscribeState => InternalSubscribeConfigurationChangeAsync(subscribeState.Descriptor, token)).ToArray();

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();
                    Logger?.LogError(ex, "恢复配置订阅出现异常");
                }
            }, token);
        }

        /// <inheritdoc/>
        protected override void SetupRequestProcessorBuilder(RequestProcessorBuilder builder)
        {
            builder.RegisterHandler(Assembly.GetExecutingAssembly())
                   .RegisterHandler(new ConfigurationChangeNotifyRequestHandler(OnConfigurationChangeNotify));
        }

        #endregion Protected 方法

        #region Private 方法

        private async Task<NacosResponse?> InternalSubscribeConfigurationChangeAsync(NacosConfigurationDescriptor descriptor, CancellationToken token)
        {
            var request = new ConfigBatchListenRequest(true).AddListenContext(descriptor);

            var response = await RequestAsync(request, token).ConfigureAwait(false);

            return response;
        }

        /// <summary>
        /// 配置变更通知回调方法
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task OnConfigurationChangeNotify(ConfigChangeNotifyRequest request)
        {
            ConfigurationChangeNotifyCallback? notifyCallback;
            ConfigurationSubscribeState? subscribeState;

            lock (_notifyCallbacks)
            {
                if (!_notifyCallbacks.TryGetValue(request, out subscribeState))
                {
                    return;
                }
                notifyCallback = subscribeState.NotifyCallback;
            }

            if (notifyCallback is not null)
            {
                Logger?.LogInformation("配置已变更 - {0}", request);

                //HACK 在此处捕获异常，是否合理
                try
                {
                    var newDescriptor = await GetConfigurationAsync(subscribeState.Descriptor, RunningToken).ConfigureAwait(false);

                    var tasks = notifyCallback.GetInvocationList()
                                              .Cast<ConfigurationChangeNotifyCallback>()
                                              .Select(callback => callback(newDescriptor, RunningToken))
                                              .ToArray();

                    await Task.WhenAll(tasks).ConfigureAwait(false);

                    await InternalSubscribeConfigurationChangeAsync(newDescriptor, RunningToken).ConfigureAwait(false);

                    subscribeState.Descriptor = newDescriptor;
                }
                catch (Exception ex)
                {
                    //HACK 是否需要异常处理
                    Logger?.LogError(ex, "配置变更订阅处理异常, 变更信息: {0}", request);
                }
            }
        }

        private async Task UnSubscribeConfigurationChange(NacosConfigurationDescriptor descriptor, ConfigurationChangeNotifyCallback notifyCallback)
        {
            if (notifyCallback is null)
            {
                return;
            }

            var configurationUniqueKey = descriptor.GetUniqueKey();

            Logger?.LogInformation("取消配置 {0} 的变更通知订阅", configurationUniqueKey);

            lock (_notifyCallbacks)
            {
                if (_notifyCallbacks.TryGetValue(descriptor, out var subscribeState))
                {
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
                    subscribeState.NotifyCallback -= notifyCallback;
#pragma warning restore CS8601 // 引用类型赋值可能为 null。
                    if (subscribeState.NotifyCallback is null)
                    {
                        _notifyCallbacks.Remove(descriptor);
                    }
                }
            }

            var request = new ConfigBatchListenRequest(false).AddListenContext(descriptor);

            //HACK 此处不做检查。。。
            await RequestAsync(request, RunningToken).ConfigureAwait(false);
        }

        #endregion Private 方法

        #region Private 类

        private class ConfigurationSubscribeState
        {
            #region Public 字段

            public ConfigurationChangeNotifyCallback NotifyCallback;

            #endregion Public 字段

            #region Public 属性

            public NacosConfigurationDescriptor Descriptor { get; set; }

            #endregion Public 属性

            #region Public 构造函数

            public ConfigurationSubscribeState(NacosConfigurationDescriptor descriptor, ConfigurationChangeNotifyCallback notifyCallback)
            {
                Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
                NotifyCallback = notifyCallback ?? throw new ArgumentNullException(nameof(notifyCallback));
            }

            #endregion Public 构造函数
        }

        #endregion Private 类
    }
}