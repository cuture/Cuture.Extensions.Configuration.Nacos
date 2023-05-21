using System;
using System.Security.Cryptography;
using System.Text;

namespace Nacos.Utils;

/// <summary>
/// Hash工具
/// </summary>
public static class HashUtil
{
    #region Public 方法

    /// <summary>
    /// 计算HMACSHA1
    /// </summary>
    /// <param name="value"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static byte[] ComputeHMACSHA1(string value, string key)
    {
        var secrectKey = Encoding.UTF8.GetBytes(key);

        using var hmac = new HMACSHA1(secrectKey);

        hmac.Initialize();

        var data = Encoding.UTF8.GetBytes(value);

        return hmac.ComputeHash(data);
    }

    /// <summary>
    /// 计算MD5
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ComputeMD5(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Array.Empty<byte>();
        }

        return ComputeMD5(Encoding.UTF8.GetBytes(value));
    }

    /// <summary>
    /// 计算MD5
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] ComputeMD5(byte[] value)
    {
        if (value is null
            || value.Length == 0)
        {
            return Array.Empty<byte>();
        }

        using var md5 = MD5.Create();

        return md5.ComputeHash(value);
    }

    /// <summary>
    /// 转换为十六进制字符串
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static string ToHexString(this byte[] hash)
    {
        var builder = new StringBuilder(32);

        foreach (byte item in hash)
        {
            builder.Append(item.ToString("x2"));
        }

        return builder.ToString();
    }

    #endregion Public 方法
}
