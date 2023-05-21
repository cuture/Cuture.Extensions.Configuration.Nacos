using System;
using System.Threading.Tasks;

using Nacos.Messages;
using Nacos.Utils;

namespace Nacos;

/// <summary>
/// 针对ACM的 <see cref="INacosRequestSigner"/>
/// </summary>
public class ACMRequestSigner : INacosRequestSigner
{
    #region Private 字段

    private readonly string _accessKey;

    private readonly string _accessKeySecret;

    #endregion Private 字段

    #region Public 构造函数

    /// <inheritdoc/>
    public ACMRequestSigner(string accessKey, string accessKeySecret)
    {
        if (string.IsNullOrWhiteSpace(accessKey))
        {
            throw new ArgumentException($"“{nameof(accessKey)}”不能为 null 或空白。", nameof(accessKey));
        }

        if (string.IsNullOrWhiteSpace(accessKeySecret))
        {
            throw new ArgumentException($"“{nameof(accessKeySecret)}”不能为 null 或空白。", nameof(accessKeySecret));
        }
        _accessKey = accessKey;
        _accessKeySecret = accessKeySecret;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public Task SignAsync(INacosRequest request)
    {
        var spasSignData = request.GetSpasSignData();

        var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        request.Headers.Timestamp = timestamp;

        spasSignData = string.IsNullOrEmpty(spasSignData)
                        ? timestamp
                        : $"{spasSignData}+{timestamp}";

        var hash = HashUtil.ComputeHMACSHA1(spasSignData, _accessKeySecret);

        request.Headers.SpasAccessKey = _accessKey;
        request.Headers.SpasSignature = Convert.ToBase64String(hash);

        return Task.CompletedTask;
    }

    #endregion Public 方法
}
