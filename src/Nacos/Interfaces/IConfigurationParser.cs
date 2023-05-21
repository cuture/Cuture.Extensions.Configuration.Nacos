using System.Collections.Generic;

namespace Nacos;

/// <summary>
/// 配置解析器
/// </summary>
public interface IConfigurationParser
{
    #region Public 方法

    /// <summary>
    /// 是否可以解析内容
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    bool CanParse(string content);

    /// <summary>
    /// 解析配置内容
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    IDictionary<string, string?> Parse(string content);

    #endregion Public 方法
}
