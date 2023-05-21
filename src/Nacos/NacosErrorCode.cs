namespace Nacos;

/// <summary>
/// Nacos错误码
/// </summary>
public enum NacosErrorCode
{
    /// <summary>
    /// 没有错误码
    /// </summary>
    None = 0,

    /// <summary>
    /// 找不到配置
    /// </summary>
    ConfigurationNotFound = 300,

    /// <summary>
    /// 连接没有注册
    /// </summary>
    ConnectionUnRegistered = 301,

    /// <summary>
    /// 禁止访问
    /// </summary>
    Forbidden = 403,
}
