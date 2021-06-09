using System.Text.Json.Serialization;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Nacos
{
    /// <summary>
    /// 客户端能力？
    /// </summary>
    public class ClientAbilities
    {
        [JsonPropertyName("configAbility")]
        public ClientConfigAbility ConfigAbility { get; set; } = new();

        [JsonPropertyName("namingAbility")]
        public ClientNamingAbility NamingAbility { get; set; } = new();

        [JsonPropertyName("remoteAbility")]
        public ClientRemoteAbility RemoteAbility { get; set; } = new();

        public class ClientConfigAbility
        {
            [JsonPropertyName("supportRemoteMetrics")]
            public bool SupportRemoteMetrics { get; set; }
        }

        public class ClientNamingAbility
        {
            [JsonPropertyName("supportDeltaPush")]
            public bool SupportDeltaPush { get; set; }

            [JsonPropertyName("supportRemoteMetric")]
            public bool SupportRemoteMetric { get; set; }
        }

        public class ClientRemoteAbility
        {
            [JsonPropertyName("supportRemoteConnection")]
            public bool SupportRemoteConnection { get; set; }
        }
    }
}