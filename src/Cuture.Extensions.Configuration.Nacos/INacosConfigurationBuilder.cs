using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Nacos;

namespace Cuture.Extensions.Configuration.Nacos;

/// <summary>
/// Nacos配置Builder
/// </summary>
public interface INacosConfigurationBuilder : IConfigurationBuilder
{
    #region Public 属性

    /// <summary>
    /// 使用的客户端
    /// </summary>
    INacosConfigurationClient Client { get; }

    /// <summary>
    /// 用于配置Nacos的 <see cref="IConfiguration"/>
    /// </summary>
    IConfiguration? Configuration { get; }

    /// <summary>
    /// 配置解析器
    /// </summary>
    IEnumerable<IConfigurationParser> ConfigurationParsers { get; }

    #endregion Public 属性
}
