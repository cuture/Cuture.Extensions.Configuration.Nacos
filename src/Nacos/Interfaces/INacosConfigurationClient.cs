using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nacos
{
    /// <summary>
    /// Nacos 配置 客户端
    /// </summary>
    public interface INacosConfigurationClient : INacosClient
    {
        #region Public 方法

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <param name="descriptor"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<NacosConfigurationDescriptor> GetConfigurationAsync(NacosConfigurationDescriptor descriptor, CancellationToken token = default);

        /// <summary>
        /// 订阅配置变更
        /// </summary>
        /// <param name="notifyCallback"></param>
        /// <param name="descriptor"></param>
        /// <param name="token">发起订阅使用的Token</param>
        /// <returns></returns>
        Task<IAsyncDisposable> SubscribeConfigurationChangeAsync(NacosConfigurationDescriptor descriptor,
                                                                 ConfigurationChangeNotifyCallback notifyCallback,
                                                                 CancellationToken token = default);

        #endregion Public 方法
    }
}