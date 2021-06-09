using System.Net.Http;

namespace Nacos.Http.Messages
{
    /// <summary>
    /// 查询配置请求
    /// </summary>
    public class QueryConfigurationRequest : NacosHttpRequest, INacosUniqueConfiguration
    {
        /// <inheritdoc/>
        public string DataId { get; private set; }

        /// <inheritdoc/>
        public string Group { get; private set; }

        /// <inheritdoc/>
        public string Namespace { get; private set; }

        /// <inheritdoc cref="QueryConfigurationRequest"/>
        public QueryConfigurationRequest(NacosConfigurationDescriptor descriptor)
        {
            Namespace = descriptor.Namespace;
            DataId = descriptor.DataId;
            Group = descriptor.Group;
        }

        /// <inheritdoc/>
        public string GetUniqueKey() => INacosUniqueConfiguration.GenerateUniqueKey(this);

        /// <inheritdoc/>
        public override HttpRequestMessage ToHttpRequestMessage(ServerUri uri)
        {
            //HACK UrlEncode？
            var query = $"tenant={Namespace}&dataId={DataId}&group={Group}";
            return new HttpRequestMessage(HttpMethod.Get, MakeUri(uri, "nacos/v1/cs/configs", query));
        }
    }
}