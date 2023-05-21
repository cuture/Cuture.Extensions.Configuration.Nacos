namespace Nacos.Exceptions;

/// <summary>
/// 访问被拒绝
/// </summary>
public class ForbiddenException : NacosException
{
    #region Public 构造函数

    /// <inheritdoc cref="ForbiddenException"/>
    public ForbiddenException()
    {
    }

    /// <inheritdoc cref="ForbiddenException"/>
    public ForbiddenException(string? message) : base(message)
    {
    }

    #endregion Public 构造函数
}
