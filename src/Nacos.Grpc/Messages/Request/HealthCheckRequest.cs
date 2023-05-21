namespace Nacos.Grpc.Messages;

/// <summary>
/// 健康检查请求
/// </summary>
public class HealthCheckRequest : NacosRequest
{
    /// <inheritdoc cref="HealthCheckRequest"/>
    public HealthCheckRequest() : base(false)
    {
    }
}
