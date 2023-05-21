namespace Nacos.Exceptions;

/// <summary>
/// 没有找到配置
/// </summary>
public class ConfigurationNotFoundException : NacosException
{
    #region Public 属性

    /// <summary>
    /// 配置描述
    /// </summary>
    public NacosConfigurationDescriptor Descriptor { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="ConfigurationNotFoundException"/>
    public ConfigurationNotFoundException(NacosConfigurationDescriptor descriptor) : base($"找不到对应配置 - {descriptor}")
    {
        Descriptor = descriptor;
    }

    /// <inheritdoc cref="ConfigurationNotFoundException"/>
    public ConfigurationNotFoundException(string @namespace, string group, string dataId) : this(new NacosConfigurationDescriptor(@namespace, dataId, group))
    {
    }

    #endregion Public 构造函数
}
