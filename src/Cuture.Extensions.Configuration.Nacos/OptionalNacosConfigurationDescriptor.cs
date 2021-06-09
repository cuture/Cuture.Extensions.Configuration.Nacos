namespace Nacos
{
    /// <summary>
    /// 有可选属性的 <see cref="NacosConfigurationDescriptor"/>
    /// </summary>
    public class OptionalNacosConfigurationDescriptor : NacosConfigurationDescriptor
    {
        #region Public 属性

        /// <summary>
        /// 是否可选
        /// </summary>
        public bool Optional { get; set; } = false;

        /// <summary>
        /// 监听配置变更
        /// </summary>
        public bool ReloadOnChange { get; set; } = true;

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="NacosConfigurationDescriptor(string, string, string, string?, string?)"/>
        public OptionalNacosConfigurationDescriptor(string @namespace, string dataId, string group = "DEFAULT_GROUP", bool optional = false)
            : base(@namespace, dataId, group, null, null)
        {
            Optional = optional;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{base.ToString()} Optional: {Optional} ReloadOnChange: {ReloadOnChange}";
        }

        #endregion Public 方法
    }
}