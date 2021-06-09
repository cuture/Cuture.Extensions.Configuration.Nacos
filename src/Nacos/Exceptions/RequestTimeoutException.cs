using System;

namespace Nacos
{
    /// <summary>
    /// 请求超时
    /// </summary>
    public class RequestTimeoutException : NacosException
    {
        #region Public 构造函数

        /// <inheritdoc cref="RequestTimeoutException"/>
        public RequestTimeoutException()
        {
        }

        /// <inheritdoc cref="RequestTimeoutException"/>
        public RequestTimeoutException(string? message) : base(message)
        {
        }

        /// <inheritdoc cref="RequestTimeoutException"/>
        public RequestTimeoutException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        #endregion Public 构造函数
    }
}