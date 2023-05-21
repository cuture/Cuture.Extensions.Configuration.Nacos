using System;

namespace Nacos;

/// <summary>
/// Nacos用户
/// </summary>
public class NacosUser
{
    #region Public 属性

    /// <summary>
    /// Account
    /// </summary>
    public string Account { get; set; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc/>
    public NacosUser(string account, string password)
    {
        if (string.IsNullOrWhiteSpace(account))
        {
            throw new ArgumentException($"“{nameof(account)}”不能为 null 或空白。", nameof(account));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException($"“{nameof(password)}”不能为 null 或空白。", nameof(password));
        }

        Account = account;
        Password = password;
    }

    #endregion Public 构造函数
}
