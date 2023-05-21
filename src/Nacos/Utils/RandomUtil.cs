using System;

namespace Nacos.Utils;

/// <summary>
/// 随机数工具
/// </summary>
public static class RandomUtil
{
    #region Private 字段

    private static readonly Random s_random = new();

    #endregion Private 字段

    #region Public 方法

    /// <summary>
    /// <see cref="Random.Next(int)"/>
    /// </summary>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int Random(int max) => s_random.Next(0, max);

    #endregion Public 方法
}
