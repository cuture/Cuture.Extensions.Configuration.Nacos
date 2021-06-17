using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;

using Microsoft.Extensions.Logging;

using Nacos;

namespace Cuture.Extensions.Configuration.Nacos
{
    /// <summary>
    /// Nacos配置源选项
    /// </summary>
    public class NacosConfigurationSourceOptions
    {
        #region Public 属性

        /// <inheritdoc cref="AliyunAcsProfile"/>
        public AliyunAcsProfile? AcsProfile { get; set; }

        /// <summary>
        /// 客户端创建委托
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Func<NacosConfigurationSourceOptions, INacosConfigurationClient>? ClientCreationFunction { get; set; }

        /// <summary>
        /// 指定自动获取客户端IP时的子网
        /// <para/> 优先级：<see cref="SpecifyClientIP"/> > <see cref="ClientIPSubnet"/>
        /// </summary>
        public string? ClientIPSubnet { get; set; }

        /// <summary>
        /// 配置解析器列表（列表顺序对应解析优先级）
        /// </summary>
        public List<IConfigurationParser> ConfigurationParsers { get; set; } = new()
        {
            new JsonConfigurationParser(),
        };

        /// <summary>
        /// 连接健康检查时间间隔
        /// </summary>
        public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// 用以记录内部日志的 <see cref="ILoggerFactory"/>
        /// </summary>
        public ILoggerFactory? LoggerFactory { get; set; }

        /// <summary>
        /// Nacos服务地址列表
        /// </summary>
        public List<ServerUri> Servers { get; } = new();

        /// <summary>
        /// 指定客户端IP
        /// <para/> 优先级：<see cref="SpecifyClientIP"/> > <see cref="ClientIPSubnet"/>
        /// </summary>
        public IPAddress? SpecifyClientIP { get; set; }

        /// <summary>
        /// 需要订阅的 配置 列表
        /// </summary>
        public List<OptionalNacosConfigurationDescriptor> Subscriptions { get; } = new();

        /// <summary>
        /// 用以登录的用户信息
        /// </summary>
        public NacosUser? User { get; set; }

        #endregion Public 属性
    }
}