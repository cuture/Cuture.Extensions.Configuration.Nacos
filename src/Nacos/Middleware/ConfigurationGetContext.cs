using System.Threading;

namespace Nacos.Middleware
{
    /// <summary>
    /// 配置获取上下文
    /// </summary>
    public class ConfigurationGetContext
    {
        #region Public 属性

        /// <summary>
        /// 取消Token
        /// </summary>
        public CancellationToken CancellationToken { get; }

        /// <summary>
        /// 使用的客户端
        /// </summary>
        public INacosConfigurationClient Client { get; }

        /// <summary>
        /// 将要获取的配置描述
        /// </summary>
        public NacosConfigurationDescriptor Descriptor { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="ConfigurationGetContext"/>
        public ConfigurationGetContext(INacosConfigurationClient client, NacosConfigurationDescriptor descriptor, CancellationToken cancellationToken)
        {
            Client = client;
            Descriptor = descriptor;
            CancellationToken = cancellationToken;
        }

        #endregion Public 构造函数
    }
}