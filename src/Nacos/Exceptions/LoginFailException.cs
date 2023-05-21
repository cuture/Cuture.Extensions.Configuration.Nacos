using System;

namespace Nacos.Exceptions;

/// <summary>
/// 登录失败
/// </summary>
public class LoginFailException : NacosException
{
    #region Public 构造函数

    /// <inheritdoc cref="LoginFailException"/>
    public LoginFailException()
    {
    }

    /// <inheritdoc cref="LoginFailException"/>
    public LoginFailException(string? message) : base(message)
    {
    }

    /// <inheritdoc cref="LoginFailException"/>
    public LoginFailException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    #endregion Public 构造函数
}
