using System.Text.Json.Serialization;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

namespace Nacos.Grpc.Messages;

/// <summary>
/// 配置变更通知请求
/// </summary>
public class ConfigChangeNotifyRequest : NacosRequest, INacosUniqueConfiguration, IConfigurationChangeNotify
{
    #region Private 字段

    private string? _uniqueKey;

    #endregion Private 字段

    #region Public 属性

    [JsonPropertyName("beta")]
    public bool Beta { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("contentPush")]
    public bool ContentPush { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("dataId")]
    public string DataId { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("group")]
    public string Group { get; set; }

    [JsonPropertyName("lastModifiedTs")]
    public long LastModifiedTimestamp { get; set; }

    /// <inheritdoc/>
    [JsonPropertyName("tenant")]
    public string Namespace { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    #endregion Public 属性

    #region Public 方法

    /// <inheritdoc/>
    public string GetUniqueKey() => _uniqueKey ??= $"{Namespace}+{Group}+{DataId}";

    /// <inheritdoc/>
    public override string ToString() => GetUniqueKey();

    #endregion Public 方法
}
