namespace Nacos;

/// <summary>
/// 配置变更通知
/// </summary>
public interface IConfigurationChangeNotify
{
    #region Public 属性

    /// <summary>
    /// 是否Beta
    /// </summary>
    bool Beta { get; }

    /// <summary>
    /// 内容
    /// </summary>
    string? Content { get; }

    /// <summary>
    /// ???
    /// </summary>
    bool ContentPush { get; }

    /// <summary>
    /// DataId
    /// </summary>
    string DataId { get; }

    /// <summary>
    /// Group
    /// </summary>
    string Group { get; }

    /// <summary>
    /// 最后修改时间戳
    /// </summary>
    long LastModifiedTimestamp { get; }

    /// <summary>
    /// 命名空间（tenant）
    /// </summary>
    string Namespace { get; }

    /// <summary>
    /// 类型
    /// </summary>
    string Type { get; }

    #endregion Public 属性
}
