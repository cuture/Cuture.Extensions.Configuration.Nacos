using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nacos.Exceptions;
using Nacos.Http.Messages;
using Nacos.Internal;
using Nacos.Middleware;
using Nacos.Utils;

namespace Nacos.Http
{
    /// <summary>
    /// Nacos Http 配置客户端
    /// </summary>
    public sealed class NacosConfigurationHttpClient : NacosHttpClient, INacosConfigurationClient
    {
        #region Private 字段

        private readonly Func<ConfigurationGetContext, Task<NacosConfigurationDescriptor>> _getConfigurationDelegate;

        private readonly Dictionary<INacosUniqueConfiguration, CancellationTokenSource> _subscribeTokenSources = new(new INacosUniqueConfigurationEqualityComparer());

        private readonly ConfigurationSubscriptionCollection _subscriptions = new();

        #endregion Private 字段

        #region Public 构造函数

        /// <inheritdoc cref="NacosConfigurationHttpClient"/>
        public NacosConfigurationHttpClient(NacosHttpConfigurationClientOptions clientOptions) : base(clientOptions)
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
            return _getConfigurationDelegate(new(this, descriptor, token));
        }

        /// <inheritdoc/>
        public Task<IAsyncDisposable> SubscribeConfigurationChangeAsync(NacosConfigurationDescriptor descriptor,
                                                                        ConfigurationChangeNotifyCallback notifyCallback,
                                                                        CancellationToken token = default)
        {
            _subscriptions.AddSubscribe(descriptor, notifyCallback);

            CancellationTokenSource? subscribeTokenSource = null;

            lock (_subscribeTokenSources)
            {
                if (_subscribeTokenSources.TryGetValue(descriptor, out subscribeTokenSource))
                {
                    return Task.FromResult<IAsyncDisposable>(CreateUnsubscriber());
                }
                else
                {
                    subscribeTokenSource = CancellationTokenSource.CreateLinkedTokenSource(RunningToken);
                    _subscribeTokenSources.Add(descriptor, subscribeTokenSource);
                }
            }

            _ = PollingListeningConfigurationAsync(descriptor, subscribeTokenSource.Token);

            return Task.FromResult<IAsyncDisposable>(CreateUnsubscriber());

            HttpConfigurationChangeUnsubscriber CreateUnsubscriber()
            {
                return new HttpConfigurationChangeUnsubscriber(descriptor, notifyCallback, UnSubscribeConfigurationChange);
            }
        }

        #endregion Public 方法

        #region Protected 方法

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _subscriptions.Dispose();

            CancellationTokenSource[] ctss;

            lock (_subscribeTokenSources)
            {
                ctss = _subscribeTokenSources.Values.ToArray();
                _subscribeTokenSources.Clear();
            }

            foreach (var item in ctss)
            {
                item.SilenceRelease();
            }

            base.Dispose(disposing);
        }

        #endregion Protected 方法

        #region Private 方法

        private async Task<NacosConfigurationDescriptor> InternalGetConfigurationAsync(NacosConfigurationDescriptor descriptor, CancellationToken token)
        {
            var request = new QueryConfigurationRequest(descriptor);
            try
            {
                var content = await RequestAsync(request, token).ConfigureAwait(false);
                return descriptor.WithContent(content, HashUtil.ComputeMD5(content).ToHexString());
            }
            catch (HttpRequestNotFoundException)
            {
                throw new ConfigurationNotFoundException(descriptor);
            }
        }

        private async Task PollingListeningConfigurationAsync(NacosConfigurationDescriptor descriptor, CancellationToken token)
        {
            var scaler = new Scaler(0, 10, 60);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var request = new ListeningConfigurationRequest(descriptor);

                    var response = await RequestAsync(request, token).ConfigureAwait(false);

                    //HACK 此处只订阅了单个配置，所以不需要从响应内容解析是哪个配置有更新

                    if (!string.IsNullOrWhiteSpace(response))
                    {
                        var hasSubscribe = _subscriptions.TryGetSubscribe(descriptor, out var state);

                        if (!hasSubscribe)
                        {
                            Logger?.LogTrace("没有对配置 {0} 的订阅, 监听任务退出.", descriptor);
                            return;
                        }

                        var notifyCallback = state?.NotifyCallback;

                        if (notifyCallback is null)
                        {
                            //HACK 理论上不应该走到这里面的逻辑

                            scaler.Add();

                            Logger?.LogInformation("监听到配置 {0} 有变更, 但没有获取到回调委托, 等待 {1} s 后重试", descriptor, scaler.Value);

                            await Task.Delay(TimeSpan.FromSeconds(scaler.Value), token).ConfigureAwait(false);

                            continue;
                        }

                        //HACK 响应值是否有内容？
                        var newDescriptor = await GetConfigurationAsync(descriptor, token).ConfigureAwait(false);

                        if (!string.IsNullOrWhiteSpace(descriptor.Hash))    //hash为空，认为是第一次订阅，不触发回调
                        {
                            //HACK 在此处捕获异常，是否合理
                            try
                            {
                                var tasks = notifyCallback.GetInvocationList()
                                                          .Cast<ConfigurationChangeNotifyCallback>()
                                                          .Select(callback => callback(newDescriptor, RunningToken))
                                                          .ToArray();

                                await Task.WhenAll(tasks).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                //HACK 是否需要异常处理
                                Logger?.LogError(ex, "配置变更订阅处理异常, 变更信息: {0}", newDescriptor);
                            }
                        }

                        descriptor = descriptor.WithContent(newDescriptor.Content, newDescriptor.Hash ?? HashUtil.ComputeMD5(newDescriptor.Content).ToHexString());
                    }

                    scaler.Reset();
                }
                catch (HttpRequestNotFoundException)
                {
                    scaler.Add();

                    Logger?.LogInformation("监听配置变更, 无法找到资源 - {0} , 等待 {1} s 后重试", descriptor, scaler.Value);

                    await Task.Delay(TimeSpan.FromSeconds(scaler.Value), token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();

                    scaler.Add();

                    Logger?.LogError(ex, "监听配置变更异常 - {0} , 等待 {1} s 后重试", descriptor, scaler.Value);

                    await Task.Delay(TimeSpan.FromSeconds(scaler.Value), token).ConfigureAwait(false);
                }
            }
        }

        private void UnSubscribeConfigurationChange(NacosConfigurationDescriptor descriptor, ConfigurationChangeNotifyCallback notifyCallback)
        {
            if (notifyCallback is null)
            {
                return;
            }

            var configurationUniqueKey = descriptor.GetUniqueKey();

            Logger?.LogInformation("取消配置 {0} 的变更通知订阅", configurationUniqueKey);

            if (_subscriptions.RemoveSubscribe(descriptor, notifyCallback))
            {
                lock (_subscribeTokenSources)
                {
                    if (_subscribeTokenSources.TryGetValue(descriptor, out var cts))
                    {
                        cts.SilenceRelease();
                    }
                }
            }
        }

        #endregion Private 方法
    }
}