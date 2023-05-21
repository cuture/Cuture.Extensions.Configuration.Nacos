namespace Nacos;

/// <summary>
/// Nacos唯一配置
/// </summary>
public interface INacosUniqueConfiguration
{
    #region Public 属性

    /// <summary>
    /// DataId
    /// </summary>
    string DataId { get; }

    /// <summary>
    /// 组
    /// </summary>
    string Group { get; }

    /// <summary>
    /// 命名空间 (tenant)
    /// </summary>
    string Namespace { get; }

    #endregion Public 属性

    #region Public 方法

    /// <summary>
    /// 生成配置唯一Key ("Namespace+Group+DataId")
    /// </summary>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static string GenerateUniqueKey(INacosUniqueConfiguration configuration) => $"{configuration.Namespace}+{configuration.Group}+{configuration.DataId}";

    /// <summary>
    /// 获取配置唯一Key ("Namespace+Group+DataId")
    /// </summary>
    /// <returns></returns>
    string GetUniqueKey();

    #endregion Public 方法
}
