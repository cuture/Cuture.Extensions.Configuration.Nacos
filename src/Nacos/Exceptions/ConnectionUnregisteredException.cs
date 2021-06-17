namespace Nacos.Exceptions
{
    /// <summary>
    /// 请求的连接没有正确注册
    /// </summary>
    public class ConnectionUnRegisteredException : NacosException
    {
        #region Public 构造函数

        /// <inheritdoc cref="ConnectionUnRegisteredException"/>
        public ConnectionUnRegisteredException()
        {
        }

        /// <inheritdoc cref="ConnectionUnRegisteredException"/>
        public ConnectionUnRegisteredException(string? message) : base(message)
        {
        }

        #endregion Public 构造函数
    }
}