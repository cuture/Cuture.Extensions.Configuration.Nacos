using System;

namespace Nacos.Exceptions;

/// <summary>
/// 未知的通讯消息
/// </summary>
public class UnknownMessageException : NacosException
{
    #region Public 构造函数

    /// <inheritdoc cref="UnknownMessageException"/>
    public UnknownMessageException()
    {
    }

    /// <inheritdoc cref="UnknownMessageException"/>
    public UnknownMessageException(string? message) : base(message)
    {
    }

    /// <inheritdoc cref="UnknownMessageException"/>
    public UnknownMessageException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    #endregion Public 构造函数
}
