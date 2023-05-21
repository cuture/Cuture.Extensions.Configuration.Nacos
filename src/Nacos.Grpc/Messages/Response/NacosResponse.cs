using System.Text.Json.Serialization;

namespace Nacos.Grpc.Messages;

/// <summary>
/// Nacos响应
/// </summary>
public abstract class NacosResponse
{
    /// <summary>
    /// ErrorCode
    /// </summary>
    [JsonPropertyName("errorCode")]
    public NacosErrorCode ErrorCode { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => ResultCode == 200;

    /// <summary>
    /// 消息
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 请求ID
    /// </summary>
    [JsonPropertyName("requestId")]
    public string? RequestId { get; set; }

    /// <summary>
    /// 响应码
    /// </summary>
    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; } = 200;
}
