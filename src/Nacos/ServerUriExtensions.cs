using System;
using System.Collections.Generic;
using System.Linq;

namespace Nacos
{
    /// <summary>
    ///
    /// </summary>
    public static class ServerUriExtensions
    {
        #region Public 方法

        /// <summary>
        /// 尝试获取列表中的Endpoint服务地址
        /// </summary>
        /// <param name="serverUris"></param>
        /// <param name="endpointUris"></param>
        /// <returns></returns>
        public static bool TryGetEndpointServerUris(this IEnumerable<ServerUri> serverUris, out ServerUri[] endpointUris)
        {
            if (serverUris is null)
            {
                throw new ArgumentNullException(nameof(serverUris));
            }

            endpointUris = serverUris.Where(m => m.IsEndpoint()).ToArray();

            if (endpointUris.Length > 0
                && serverUris.Count() != endpointUris.Length)
            {
                throw new NacosException("endpoint地址和Nacos地址不能混用");
            }

            var acmCount = endpointUris.Count(m => m.IsAliyunAcm());
            if (acmCount > 0
                && acmCount != endpointUris.Length)
            {
                throw new NacosException("endpoint地址和acm地址不能混用");
            }

            return endpointUris.Length > 0;
        }

        #endregion Public 方法
    }
}