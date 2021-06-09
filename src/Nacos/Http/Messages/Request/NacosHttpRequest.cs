using System;
using System.Net.Http;

namespace Nacos.Http.Messages
{
    /// <summary>
    /// Nacos的Http请求
    /// </summary>
    public abstract class NacosHttpRequest
    {
        #region Private 方法

        /// <summary>
        /// 获取等价的<see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public abstract HttpRequestMessage ToHttpRequestMessage(ServerUri uri);

        #endregion Private 方法

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
    }
}