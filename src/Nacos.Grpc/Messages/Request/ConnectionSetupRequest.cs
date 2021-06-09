using System.Text.Json.Serialization;

namespace Nacos.Grpc.Messages
{
    /// <summary>
    /// 设置连接请求
    /// </summary>
    public class ConnectionSetupRequest : NacosRequest
    {
        /// <summary>
        /// 客户端能力
        /// </summary>
        [JsonPropertyName("abilities")]
        public ClientAbilities? Abilities { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        [JsonPropertyName("clientIp")]
        public string? ClientIp { get; set; }

        /// <summary>
        /// 客户端版本
        /// </summary>
        [JsonPropertyName("clientVersion")]
        public string? ClientVersion { get; set; }

        /// <summary>
        /// 命名空间
        /// </summary>
        [JsonPropertyName("tenant")]
        public string? Namespace { get; set; }
    }
}