using System;

using Nacos.Grpc;

namespace Nacos;

/// <summary>
/// NacosGrpc客户端选项
/// </summary>
public class NacosGrpcClientOptions : NacosClientOptions
{
    #region Public 属性

    /// <summary>
    /// 客户端能力
    /// </summary>
    public ClientAbilities? ClientAbilities { get; set; }

    /// <summary>
    /// 健康检查间隔
    /// </summary>
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <inheritdoc cref="IHostAddressAccessor"/>
    public IHostAddressAccessor HostAddressAccessor { get; set; }

    /// <summary>
    /// 用于通信的消息序列化器
    /// </summary>
    public IMessageSerializer? MessageSerializer { get; set; }

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
        : base(clientName, serverAddressAccessor, httpClientFactory)
    {
        HostAddressAccessor = hostAddressAccessor ?? new AutomaticHostAddressAccessor();
    }

    #endregion Public 构造函数
}
