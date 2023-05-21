using System.Threading.Tasks;

using Nacos.Grpc.Messages;

namespace Nacos.Grpc;

/// <summary>
/// 请求处理器
/// </summary>
public interface IRequestHandler
{
}

/// <summary>
/// 请求处理器
/// </summary>
/// <typeparam name="TRequest"></typeparam>
public interface IRequestHandler<TRequest> : IRequestHandler where TRequest : NacosRequest
{
    #region Public 方法

    /// <summary>
    /// 处理
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<NacosResponse> HandleAsync(TRequest request);

    #endregion Public 方法
}
