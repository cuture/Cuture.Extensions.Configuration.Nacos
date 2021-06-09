using System.Text.Json.Serialization;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Nacos.Grpc.Messages
{
    public class ConfigQueryRequest : NacosRequest
    {
        #region Public 属性

        [JsonPropertyName("dataId")]
        public string DataId { get; private set; }

        [JsonPropertyName("group")]
        public string Group { get; private set; }

        [JsonPropertyName("tenant")]
        public string Namespace { get; private set; }

        #endregion Public 属性

        #region Public 构造函数

        public ConfigQueryRequest(NacosConfigurationDescriptor descriptor)
        {
            Namespace = descriptor.Namespace;
            DataId = descriptor.DataId;
            Group = descriptor.Group;
        }

        #endregion Public 构造函数
    }
}