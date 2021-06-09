using System;

namespace Nacos
{
    /// <summary>
    /// http请求404
    /// </summary>
    public class HttpRequestNotFoundException : NacosException
    {
        #region Public 构造函数

        /// <inheritdoc cref="HttpRequestNotFoundException"/>
        public HttpRequestNotFoundException(object? request = null)
        {
            if (request is not null)
            {
                Data.Add(nameof(request), request);
            }
        }

        /// <inheritdoc cref="HttpRequestNotFoundException"/>
        public HttpRequestNotFoundException(string? message, object? request = null) : base(message)
        {
            if (request is not null)
            {
                Data.Add(nameof(request), request);
            }
        }

        /// <inheritdoc cref="HttpRequestNotFoundException"/>
        public HttpRequestNotFoundException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        #endregion Public 构造函数
    }
}