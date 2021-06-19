using System.Collections.Generic;

using Nacos.Middleware;

namespace Nacos
{
    /// <summary>
    /// NacosGrpc配置客户端选项
    /// </summary>
    public class NacosGrpcConfigurationClientOptions : NacosGrpcClientOptions
    {
        #region Public 属性

        /// <summary>
        /// Nacos配置客户端的中间件列表
        /// </summary>
        public List<INacosConfigurationClientMiddleware> ConfigurationClientMiddlewares { get; } = new();

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="NacosGrpcClientOptions"/>
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="serverAddressAccessor"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="hostAddressAccessor"></param>
        public NacosGrpcConfigurationClientOptions(string clientName,
                                                   IServerAddressAccessor serverAddressAccessor,
                                                   INacosUnderlyingHttpClientFactory? httpClientFactory = null,
                                                   IHostAddressAccessor? hostAddressAccessor = null)
            : base(clientName, serverAddressAccessor, httpClientFactory, hostAddressAccessor)
        {
        }

        #endregion Public 构造函数
    }
}