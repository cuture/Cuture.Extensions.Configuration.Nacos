using System.Threading.Tasks;

using Nacos.Messages;

namespace Nacos;

/// <summary>
/// Nacos请求 签名器
/// </summary>
public interface INacosRequestSigner
{
    #region Public 方法

    /// <summary>
    /// 签名请求
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task SignAsync(INacosRequest request);

    #endregion Public 方法
}
