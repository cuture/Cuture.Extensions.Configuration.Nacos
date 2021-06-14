using System;

using Microsoft.Extensions.Logging;

using Nacos.Grpc;

namespace Nacos
{
    /// <summary>
    /// NacosGrpc客户端选项
    /// </summary>
    public class NacosGrpcClientOptions
    {
        #region Public 属性

        /// <summary>
        /// 客户端能力
        /// </summary>
        public ClientAbilities? ClientAbilities { get; set; }

        /// <summary>
        /// 客户端名称
        /// </summary>
        public string ClientName { get; }

        /// <summary>
        /// 健康检查间隔
        /// </summary>
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <inheritdoc cref="IHostAddressAccessor"/>
        public IHostAddressAccessor HostAddressAccessor { get; set; }

        /// <inheritdoc cref="INacosUnderlyingHttpClientFactory"/>
        public INacosUnderlyingHttpClientFactory HttpClientFactory { get; }

        /// <summary>
        /// 用于内部日志记录的 <see cref="ILoggerFactory"/>
        /// </summary>
        public ILoggerFactory? LoggerFactory { get; set; }

        /// <summary>
        /// 用于通信的消息序列化器
        /// </summary>
        public IMessageSerializer? MessageSerializer { get; set; }

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
        /// <inheritdoc cref="NacosGrpcClientOptions"/>
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="serverAddressAccessor"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="hostAddressAccessor"></param>
        public NacosGrpcClientOptions(string clientName,
                                      IServerAddressAccessor serverAddressAccessor,
                                      INacosUnderlyingHttpClientFactory? httpClientFactory = null,
                                      IHostAddressAccessor? hostAddressAccessor = null)
        {
            if (string.IsNullOrWhiteSpace(clientName))
            {
                throw new ArgumentException($"“{nameof(clientName)}”不能为 null 或空白。", nameof(clientName));
            }

            ClientName = clientName;
            ServerAddressAccessor = serverAddressAccessor ?? throw new ArgumentNullException(nameof(serverAddressAccessor));

            HttpClientFactory = httpClientFactory ?? DefaultNacosUnderlyingHttpClientFactory.Shared;
            HostAddressAccessor = hostAddressAccessor ?? new AutomaticHostAddressAccessor();
        }

        #endregion Public 构造函数
    }
}