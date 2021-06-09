using System.Text.Json.Serialization;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

namespace Nacos.Grpc.Messages
{
    public class ConfigQueryResponse : NacosResponse
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("encryptedDataKey")]
        public string EncryptedDataKey { get; set; }

        [JsonPropertyName("isBeta")]
        public bool IsBeta { get; set; }

        [JsonPropertyName("lastModified")]
        public long LastModified { get; set; }

        [JsonPropertyName("md5")]
        public string Md5 { get; set; }

        [JsonPropertyName("tag")]
        public string Tag { get; set; }
    }
}