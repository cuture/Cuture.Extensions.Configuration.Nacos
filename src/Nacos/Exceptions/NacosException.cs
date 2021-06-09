using System;

namespace Nacos
{
    /// <summary>
    /// Nacos异常
    /// </summary>
    public class NacosException : Exception
    {
        #region Public 构造函数

        /// <inheritdoc cref="NacosException"/>
        public NacosException()
        {
        }

        /// <inheritdoc cref="NacosException"/>
        public NacosException(string? message) : base(message)
        {
        }

        /// <inheritdoc cref="NacosException"/>
        public NacosException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        #endregion Public 构造函数
    }
}