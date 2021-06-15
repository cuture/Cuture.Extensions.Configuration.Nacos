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
        /// 尝试获取列表中的ACM服务地址
        /// </summary>
        /// <param name="serverUris"></param>
        /// <param name="acmServerUris"></param>
        /// <returns></returns>
        public static bool TryGetAcmServerUris(this IEnumerable<ServerUri> serverUris, out ServerUri[] acmServerUris)
        {
            if (serverUris is null)
            {
                throw new ArgumentNullException(nameof(serverUris));
            }

            acmServerUris = serverUris.Where(m => m.IsAliyunAcm()).ToArray();

            if (acmServerUris.Length > 0
                && serverUris.Count() != acmServerUris.Length)
            {
                throw new NacosException("ACM地址和Nacos地址不能混用");
            }

            return acmServerUris.Length > 0;
        }

        #endregion Public 方法
    }
}