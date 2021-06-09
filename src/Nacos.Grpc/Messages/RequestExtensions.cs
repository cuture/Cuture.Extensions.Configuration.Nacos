namespace Nacos.Grpc.Messages
{
    internal static class RequestExtensions
    {
        #region Public 方法

        public static ConfigBatchListenRequest AddListenContext(this ConfigBatchListenRequest request, NacosConfigurationDescriptor descriptor)
        {
            return request.AddListenContext(descriptor.Namespace, descriptor.DataId, descriptor.Hash ?? string.Empty, descriptor.Group);
        }

        #endregion Public 方法
    }
}