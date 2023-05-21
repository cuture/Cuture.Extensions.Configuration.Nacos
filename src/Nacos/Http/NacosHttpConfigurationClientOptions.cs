using System.Collections.Generic;

using Nacos.Middleware;

namespace Nacos.Http;

/// <summary>
/// NacosHttp配置客户端选项
/// </summary>
public class NacosHttpConfigurationClientOptions : NacosHttpClientOptions
{
    #region Public 属性

    /// <summary>
    /// Nacos配置客户端的中间件列表
    /// </summary>
    public List<INacosConfigurationClientMiddleware> ConfigurationClientMiddlewares { get; } = new();

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="NacosHttpClientOptions"/>
    public NacosHttpConfigurationClientOptions(string clientName,
                                               IServerAddressAccessor serverAddressAccessor,
                                               INacosUnderlyingHttpClientFactory? httpClientFactory = null)
        : base(clientName, serverAddressAccessor, httpClientFactory)
    {
    }

    #endregion Public 构造函数
}
