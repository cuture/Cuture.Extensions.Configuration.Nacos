using System;
using System.Text.Json.Serialization;

using Nacos.Messages;
using Nacos.Utils;

namespace Nacos.Grpc.Messages;

/// <summary>
/// Nacos请求
/// </summary>
public abstract class NacosRequest : INacosRequest
{
    #region Public 属性

    /// <summary>
    /// 请求头
    /// </summary>
    [JsonIgnore]
    public NacosHeaders Headers { get; set; }

    /// <summary>
    /// 请求ID
    /// </summary>
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="NacosRequest"/>
    public NacosRequest() : this(true)
    {
    }

    /// <summary>
    /// <inheritdoc cref="NacosRequest"/>
    /// </summary>
    /// <param name="setGenericHeaders">是否设置通用头部</param>
    public NacosRequest(bool setGenericHeaders = false)
    {
        Headers = new(setGenericHeaders);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public virtual string? GetSpasSignData() => string.Empty;

    #endregion Public 方法
}
