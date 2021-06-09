using Nacos.Grpc.GrpcService;
using Nacos.Grpc.Messages;

namespace Nacos.Grpc
{
    /// <summary>
    /// 消息序列化器
    /// </summary>
    public interface IMessageSerializer
    {
        #region Public 方法

        /// <summary>
        /// 反序列化Grpc消息到对象
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        object Deserialize(Payload payload);

        /// <summary>
        /// 序列化请求到Grpc消息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Payload Serialize(NacosRequest request);

        /// <summary>
        /// 序列化响应到Grpc消息
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        Payload Serialize(NacosResponse response);

        #endregion Public 方法
    }
}