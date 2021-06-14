using System;

using Microsoft.Extensions.Logging;

namespace Nacos.Http
{
    /// <summary>
    /// NacosHttp客户端选项
    /// </summary>
    public class NacosHttpClientOptions
    {
        #region Public 属性

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ClientName { get; }

        /// <inheritdoc cref="INacosUnderlyingHttpClientFactory"/>
        public INacosUnderlyingHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// 用于内部日志记录的 <see cref="ILoggerFactory"/>
        /// </summary>
        public ILoggerFactory? LoggerFactory { get; set; }

        /// <summary>
        /// 服务地址访问器
        /// </summary>
        public IServerAddressAccessor ServerAddressAccessor { get; }

        /// <summary>
        /// 用于登录的用户
        /// </summary>
        public NacosUser? User { get; set; }

        #endregion Public 属性

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="NacosHttpClientOptions"/>
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="serverAddressAccessor"></param>
        /// <param name="httpClientFactory"></param>
        public NacosHttpClientOptions(string clientName, IServerAddressAccessor serverAddressAccessor, INacosUnderlyingHttpClientFactory? httpClientFactory = null)
        {
            if (string.IsNullOrWhiteSpace(clientName))
            {
                throw new ArgumentException($"“{nameof(clientName)}”不能为 null 或空白。", nameof(clientName));
            }

            ClientName = clientName;
            ServerAddressAccessor = serverAddressAccessor ?? throw new ArgumentNullException(nameof(serverAddressAccessor));
            HttpClientFactory = httpClientFactory ?? DefaultNacosUnderlyingHttpClientFactory.Shared;
        }

        #endregion Public 构造函数
    }
}