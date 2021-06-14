using System;
using System.Net.Http;

namespace Nacos
{
    /// <summary>
    /// Nacos底层<see cref="HttpClient"/>工厂
    /// </summary>
    public interface INacosUnderlyingHttpClientFactory : IDisposable
    {
        #region Public 方法

        /// <summary>
        /// 创建 <see cref="HttpClient"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        HttpClient CreateClient(Uri uri);

        #endregion Public 方法
    }
}