using System.Collections.Generic;
using System.Net.Http;

namespace Nacos.Http.Messages
{
    /// <summary>
    /// 监听配置请求
    /// </summary>
    public class ListeningConfigurationRequest : NacosHttpRequest, INacosUniqueConfiguration
    {
        private static readonly string s_configurationSeparator = char.ConvertFromUtf32(1);
        private static readonly string s_fieldSeparator = char.ConvertFromUtf32(2);

        /// <inheritdoc/>
        public string DataId { get; set; }

        /// <inheritdoc/>
        public string Group { get; set; }

        /// <summary>
        /// 配置的Hash（Md5）
        /// </summary>
        public string? Hash { get; }

        /// <summary>
        /// 长轮训等待时间毫秒
        /// </summary>
        public uint LongPullingTimeout { get; set; }

        /// <inheritdoc/>
        public string Namespace { get; set; }

        /// <inheritdoc cref="QueryConfigurationRequest"/>
        public ListeningConfigurationRequest(NacosConfigurationDescriptor descriptor, uint longPullingTimeout = 30_000)
        {
            Namespace = descriptor.Namespace;
            DataId = descriptor.DataId;
            Group = descriptor.Group;
            Hash = descriptor.Hash;
            LongPullingTimeout = longPullingTimeout;
        }

        /// <inheritdoc/>
        public string GetUniqueKey() => INacosUniqueConfiguration.GenerateUniqueKey(this);

        /// <inheritdoc/>
        public override HttpRequestMessage ToHttpRequestMessage(ServerUri uri)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, MakeUri(uri, "nacos/v1/cs/configs/listener", $"?tenant={Namespace}"))
            {
                Content = new FormUrlEncodedContent(new[] { new KeyValuePair<string?, string?>("Listening-Configs", GetFormData()) })
            };

            message.Headers.TryAddWithoutValidation("Long-Pulling-Timeout", LongPullingTimeout > 0 ? LongPullingTimeout.ToString() : "30000");

            return message;
        }

        private string GetFormData()
        {
            return string.IsNullOrEmpty(Namespace)
                        ? $"{DataId}{s_fieldSeparator}{Group}{s_fieldSeparator}{Hash}{s_configurationSeparator}"
                        : $"{DataId}{s_fieldSeparator}{Group}{s_fieldSeparator}{Hash}{s_fieldSeparator}{Namespace}{s_configurationSeparator}";
        }
    }
}