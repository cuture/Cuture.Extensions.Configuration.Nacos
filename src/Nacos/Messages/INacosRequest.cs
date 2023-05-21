using System.Text.Json.Serialization;

namespace Nacos.Messages;

/// <summary>
/// Nacos请求
/// </summary>
public interface INacosRequest
{
    #region Public 属性

    /// <summary>
    /// 请求头
    /// </summary>
    [JsonIgnore]
    public NacosHeaders Headers { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 获取用于Spas签名的数据
    /// </summary>
    /// <returns></returns>
    string? GetSpasSignData();

    #endregion Public 方法
}
