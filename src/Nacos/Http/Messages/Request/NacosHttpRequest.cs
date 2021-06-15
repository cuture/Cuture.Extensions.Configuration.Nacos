using System;
using System.Net.Http;
using System.Text.Json.Serialization;

using Nacos.Messages;

namespace Nacos.Http.Messages
{
    /// <summary>
    /// Nacos的Http请求
    /// </summary>
    public abstract class NacosHttpRequest : INacosRequest
    {
        #region Public 属性

        /// <inheritdoc/>
        [JsonIgnore]
        public NacosHeaders Headers { get; } = new(false);

        #endregion Public 属性

        #region Private 方法

        /// <summary>
        /// 获取等价的<see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public abstract HttpRequestMessage ToHttpRequestMessage(ServerUri uri);

        #endregion Private 方法

        #region Public 方法

        /// <inheritdoc/>
        public virtual string? GetSpasSignData() => string.Empty;

        #endregion Public 方法

        #region Protected 方法

        /// <summary>
        /// 生成 <see cref="Uri"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="path"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        protected static Uri MakeUri(ServerUri uri, string path, string? query = null)
        {
            query = string.IsNullOrEmpty(query)
                        ? query
                        : query.StartsWith('?')
                            ? query
                            : $"?{query}";

            return new UriBuilder(uri.Scheme, uri.Host, uri.HttpPort, path, query).Uri;
        }

        #endregion Protected 方法
    }
}