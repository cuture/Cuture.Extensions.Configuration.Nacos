using System;

namespace Nacos
{
    /// <summary>
    /// 配置订阅状态
    /// </summary>
    public class ConfigurationSubscribeState
    {
        #region Public 字段

        /// <summary>
        /// 配置变更回调
        /// </summary>
        public ConfigurationChangeNotifyCallback NotifyCallback;

        #endregion Public 字段

        #region Public 属性

        /// <summary>
        /// 配置描述
        /// </summary>
        public NacosConfigurationDescriptor Descriptor { get; set; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="ConfigurationSubscribeState"/>
        public ConfigurationSubscribeState(NacosConfigurationDescriptor descriptor, ConfigurationChangeNotifyCallback notifyCallback)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            NotifyCallback = notifyCallback ?? throw new ArgumentNullException(nameof(notifyCallback));
        }

        #endregion Public 构造函数
    }
}