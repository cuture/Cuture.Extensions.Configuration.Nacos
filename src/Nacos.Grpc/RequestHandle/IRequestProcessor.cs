using System.Threading.Tasks;

using Nacos.Grpc.Messages;

namespace Nacos.Grpc
{
    /// <summary>
    /// 请求处理器
    /// </summary>
    public interface IRequestProcessor
    {
        #region Public 方法

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<NacosResponse> HandleAsync(NacosRequest request);

        #endregion Public 方法
    }
}