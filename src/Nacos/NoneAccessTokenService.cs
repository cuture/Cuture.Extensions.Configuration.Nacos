using System.Threading.Tasks;

namespace Nacos;

/// <summary>
/// NacosToken服务
/// </summary>
public sealed class NoneAccessTokenService : IAccessTokenService
{
    #region Public 属性

    /// <inheritdoc/>
    public string AccessToken => string.Empty;

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public Task InitAsync() => Task.CompletedTask;

    /// <inheritdoc/>
    public Task<string> RefreshAccessTokenAsync() => Task.FromResult(string.Empty);

    #endregion Public 方法
}
