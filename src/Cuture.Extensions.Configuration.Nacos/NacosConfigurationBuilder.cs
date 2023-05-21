using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

using Nacos;

namespace Cuture.Extensions.Configuration.Nacos;

internal class NacosConfigurationBuilder : INacosConfigurationBuilder
{
    #region Private 字段

    private readonly IConfigurationBuilder _configurationBuilder;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public INacosConfigurationClient Client { get; }

    /// <inheritdoc/>
    public IConfiguration? Configuration { get; }

    /// <inheritdoc/>
    public IEnumerable<IConfigurationParser> ConfigurationParsers { get; set; }

    #endregion Public 属性

    #region Public 构造函数

    public NacosConfigurationBuilder(IConfigurationBuilder configurationBuilder,
                                     IConfiguration? configuration,
                                     INacosConfigurationClient client,
                                     IEnumerable<IConfigurationParser> configurationParsers)
    {
        _configurationBuilder = configurationBuilder ?? throw new ArgumentNullException(nameof(configurationBuilder));
        Configuration = configuration;
        Client = client ?? throw new ArgumentNullException(nameof(client));
        ConfigurationParsers = configurationParsers ?? throw new ArgumentNullException(nameof(configurationParsers));
    }

    #endregion Public 构造函数

    #region IConfigurationBuilder

    public IDictionary<string, object> Properties => _configurationBuilder.Properties;

    public IList<IConfigurationSource> Sources => _configurationBuilder.Sources;

    public IConfigurationBuilder Add(IConfigurationSource source)
    {
        return _configurationBuilder.Add(source);
    }

    public IConfigurationRoot Build()
    {
        return _configurationBuilder.Build();
    }

    #endregion IConfigurationBuilder
}
