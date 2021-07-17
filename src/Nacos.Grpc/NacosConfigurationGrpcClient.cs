using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nacos.Exceptions;
using Nacos.Grpc.Messages;
using Nacos.Middleware;

namespace Nacos.Grpc
{
    /// <summary>
    /// Nacos Grpc 配置客户端
    /// </summary>
    public sealed class NacosConfigurationGrpcClient : NacosGrpcClient, INacosConfigurationClient
    {
        #region Private 字段

        private readonly Func<ConfigurationGetContext, Task<NacosConfigurationDescriptor>> _getConfigurationDelegate;

        private readonly ConfigurationSubscriptionCollection _subscriptions = new();

        #endregion Private 字段

        #region Public 构造函数

        /// <inheritdoc cref="NacosConfigurationGrpcClient"/>
        public NacosConfigurationGrpcClient(NacosGrpcConfigurationClientOptions clientOptions) : base(clientOptions)
        {
            var middlewares = clientOptions.ConfigurationClientMiddlewares;
            _getConfigurationDelegate = middlewares.BuildGetConfigurationDelegateWithAliyunKMSDecrypt(acsProfile: clientOptions.AcsProfile,
                                                                                                      loggerFactory: clientOptions.LoggerFactory,
                                                                                                      httpClientFactory: clientOptions.HttpClientFactory,
                                                                                                      endpointDelegate: context => InternalGetConfigurationAsync(context.Descriptor, context.CancellationToken));
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public Task<NacosConfigurationDescriptor> GetConfigurationAsync(NacosConfigurationDescriptor descriptor, CancellationToken token = default)
        {
            CheckInitiated();
            CheckDisposed();

            return _getConfigurationDelegate(new(this, descriptor, token));
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

            _subscriptions.AddSubscribe(descriptor, notifyCallback);

            return new ConfigurationChangeUnsubscriber(descriptor, notifyCallback, UnSubscribeConfigurationChange);
        }

        #endregion Public 方法

        #region Protected 方法

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _subscriptions.Dispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void OnConnectionRestore(CancellationToken token)
        {
            Logger?.LogInformation("连接已恢复");

            var subscriptions = _subscriptions.GetAllSubscription();

            _ = Task.Run(async () =>
            {
                try
                {
                    var tasks = subscriptions.Select(subscribeState => InternalSubscribeConfigurationChangeAsync(subscribeState.Descriptor, token)).ToArray();

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

        private async Task<NacosConfigurationDescriptor> InternalGetConfigurationAsync(NacosConfigurationDescriptor descriptor, CancellationToken token)
        {
            var request = new ConfigQueryRequest(descriptor);
            request.Headers.Add("notify", "false");

            var response = await RequestAsync<ConfigQueryResponse>(request, token).ConfigureAwait(false);

            if (response.ErrorCode == NacosErrorCode.ConfigurationNotFound)
            {
                throw new ConfigurationNotFoundException(descriptor);
            }

            return descriptor.WithContent(response.Content, response.Md5);
        }

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
            if (_subscriptions.TryGetSubscribe(request, out var subscribeState))
            {
                Logger?.LogInformation("配置已变更 - {0}", request);

                var notifyCallback = subscribeState.NotifyCallback;

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

            _subscriptions.RemoveSubscribe(descriptor, notifyCallback);

            var request = new ConfigBatchListenRequest(false).AddListenContext(descriptor);

            //HACK 此处不做检查。。。
            await RequestAsync(request, RunningToken).ConfigureAwait(false);
        }

        #endregion Private 方法
    }
}