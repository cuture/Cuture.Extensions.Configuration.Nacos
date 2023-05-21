using System;

namespace Nacos;

/// <summary>
/// 阿里云 ACS 访问信息
/// </summary>
public class AliyunAcsProfile
{
    #region Public 属性

    /// <summary>
    /// AccessKeyId
    /// </summary>
    public string AccessKeyId { get; }

    /// <summary>
    /// AccessKeySecret
    /// </summary>
    public string AccessKeySecret { get; }

    /// <summary>
    /// RegionId
    /// </summary>
    public string RegionId { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="AliyunAcsProfile"/>
    public AliyunAcsProfile(string regionId, string accessKeyId, string accessKeySecret)
    {
        if (string.IsNullOrWhiteSpace(regionId))
        {
            throw new ArgumentException($"“{nameof(regionId)}”不能为 null 或空白。", nameof(regionId));
        }

        if (string.IsNullOrWhiteSpace(accessKeyId))
        {
            throw new ArgumentException($"“{nameof(accessKeyId)}”不能为 null 或空白。", nameof(accessKeyId));
        }

        if (string.IsNullOrWhiteSpace(accessKeySecret))
        {
            throw new ArgumentException($"“{nameof(accessKeySecret)}”不能为 null 或空白。", nameof(accessKeySecret));
        }

        RegionId = regionId;
        AccessKeyId = accessKeyId;
        AccessKeySecret = accessKeySecret;
    }

    #endregion Public 构造函数
}
