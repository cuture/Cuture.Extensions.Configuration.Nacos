using System;
using System.Threading.Tasks;

namespace Nacos;

/// <summary>
/// NacosToken服务
/// </summary>
public interface IAccessTokenService : IDisposable
{
    #region Public 属性

    /// <summary>
    /// AccessToken
    /// </summary>
    string AccessToken { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    Task InitAsync();

    /// <summary>
    /// 刷新Token
    /// </summary>
    /// <returns></returns>
    Task<string> RefreshAccessTokenAsync();

    #endregion Public 方法
}
