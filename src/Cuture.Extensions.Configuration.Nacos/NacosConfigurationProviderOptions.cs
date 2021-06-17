using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;

using Nacos;
using Nacos.Exceptions;

namespace Cuture.Extensions.Configuration.Nacos
{
    /// <summary>
    /// Nacos 配置提供器选项
    /// </summary>
    public class NacosConfigurationProviderOptions
    {
        #region Public 属性

        /// <summary>
        /// 使用的Nacos配置客户端
        /// </summary>
        public INacosConfigurationClient Client { get; }

        /// <summary>
        /// 配置描述
        /// </summary>
        public OptionalNacosConfigurationDescriptor Descriptor { get; }

        /// <summary>
        /// 用于记录日志的 <see cref="ILoggerFactory"/>
        /// </summary>
        public ILoggerFactory? LoggerFactory { get; }

        /// <summary>
        /// 配置转换器列表
        /// </summary>
        public IEnumerable<IConfigurationParser> Parsers { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="NacosConfigurationProviderOptions"/>
        public NacosConfigurationProviderOptions(INacosConfigurationClient client,
                                                 OptionalNacosConfigurationDescriptor descriptor,
                                                 IEnumerable<IConfigurationParser> parsers,
                                                 ILoggerFactory? loggerFactory = null)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            Parsers = parsers ?? throw new ArgumentNullException(nameof(parsers));

            if (!Parsers.Any())
            {
                throw new NacosException($"必须配置有效的解析器");
            }

            LoggerFactory = loggerFactory;
        }

        #endregion Public 构造函数
    }
}