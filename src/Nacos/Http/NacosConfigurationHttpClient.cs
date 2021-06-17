using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nacos.Exceptions;
using Nacos.Http.Messages;
using Nacos.Internal;
using Nacos.Utils;

namespace Nacos.Http
{
    /// <summary>
    /// Nacos Http 配置客户端
    /// </summary>
    public sealed class NacosConfigurationHttpClient : NacosHttpClient, INacosConfigurationClient
    {
        #region Public 构造函数

        /// <inheritdoc cref="NacosConfigurationHttpClient"/>
        public NacosConfigurationHttpClient(NacosHttpClientOptions clientOptions) : base(clientOptions)
        {
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public async Task<NacosConfigurationDescriptor> GetConfigurationAsync(NacosConfigurationDescriptor descriptor, CancellationToken token = default)
        {
            var request = new QueryConfigurationRequest(descriptor);
            try
            {
                var content = await RequestAsync(request, token).ConfigureAwait(false);
                return descriptor.WithContent(content);
            }
            catch (HttpRequestNotFoundException)
            {
                throw new ConfigurationNotFoundException(descriptor);
            }
        }

        /// <inheritdoc/>
        public Task<IAsyncDisposable> SubscribeConfigurationChangeAsync(NacosConfigurationDescriptor descriptor,
                                                                        ConfigurationChangeNotifyCallback notifyCallback,
                                                                        CancellationToken token = default)
        {
            var pollingTokenSource = CancellationTokenSource.CreateLinkedTokenSource(RunningToken);
            pollingTokenSource.Token.Register(() => pollingTokenSource.Dispose());

            _ = PollingListeningConfigurationAsync(descriptor, notifyCallback, pollingTokenSource.Token);

            var unsubscriber = new HttpConfigurationChangeUnsubscriber(pollingTokenSource);

            return Task.FromResult<IAsyncDisposable>(unsubscriber);
        }

        #endregion Public 方法

        #region Protected 方法

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion Protected 方法

        #region Private 方法

        private async Task PollingListeningConfigurationAsync(NacosConfigurationDescriptor descriptor, ConfigurationChangeNotifyCallback notifyCallback, CancellationToken token)
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
                        //HACK 响应值是否有内容？
                        var newDescriptor = await GetConfigurationAsync(descriptor, RunningToken).ConfigureAwait(false);

                        await notifyCallback(newDescriptor, token).ConfigureAwait(false);

                        descriptor = descriptor.WithContent(newDescriptor.Content, HashUtil.ComputeMD5(newDescriptor.Content).ToHexString());
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

        #endregion Private 方法
    }
}