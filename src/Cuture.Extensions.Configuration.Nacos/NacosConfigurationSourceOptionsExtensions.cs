using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Nacos;
using Nacos.Http;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    ///
    /// </summary>
    public static partial class NacosConfigurationSourceOptionsExtensions
    {
        #region HttpClient

        /// <summary>
        /// 使用Http客户端
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions UseHttpClient(this NacosConfigurationSourceOptions options)
        {
            options.ClientCreationFunction = CreateHttpConfigurationClient;
            return options;
        }

        internal static INacosConfigurationClient CreateHttpConfigurationClient(NacosConfigurationSourceOptions options)
        {
            var serverAddressAccessor = new FixedServerAddressAccessor(options.Servers.ToArray());

            var clientOptions = new NacosHttpClientOptions($"NacosHttpClient-{Guid.NewGuid():n}", serverAddressAccessor)
            {
                LoggerFactory = options.LoggerFactory,
                User = options.User,
            };

            return new NacosConfigurationHttpClient(clientOptions);
        }

        #endregion HttpClient

        /// <summary>
        /// 添加服务地址
        /// </summary>
        /// <param name="options"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions AddServerAddress(this NacosConfigurationSourceOptions options, string url)
        {
            options.Servers.Add(ServerUri.Parse(url));
            return options;
        }

        /// <summary>
        /// 添加服务地址
        /// </summary>
        /// <param name="options"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions AddServerAddress(this NacosConfigurationSourceOptions options, ServerUri uri)
        {
            options.Servers.Add(uri);
            return options;
        }

        /// <summary>
        /// 添加服务地址
        /// </summary>
        /// <param name="options"></param>
        /// <param name="uris"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions AddServerAddress(this NacosConfigurationSourceOptions options, IEnumerable<ServerUri> uris)
        {
            options.Servers.AddRange(uris);
            return options;
        }

        /// <summary>
        /// 使用 LoggerFactory
        /// </summary>
        /// <param name="options"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions UseLoggerFactory(this NacosConfigurationSourceOptions options, ILoggerFactory? loggerFactory)
        {
            options.LoggerFactory = loggerFactory;
            return options;
        }

        /// <summary>
        /// 使用用户
        /// </summary>
        /// <param name="options"></param>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions WithUser(this NacosConfigurationSourceOptions options, string account, string password)
        {
            options.User = new NacosUser(account, password);
            return options;
        }

        #region LoadConfiguration

        /// <summary>
        /// 订阅 Nacos 配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions SubscribeConfiguration(this NacosConfigurationSourceOptions options, OptionalNacosConfigurationDescriptor descriptor)
        {
            options.Subscriptions.Add(descriptor);
            return options;
        }

        /// <summary>
        /// 订阅 Nacos 配置
        /// </summary>
        /// <param name="options"></param>
        /// <param name="namespace"></param>
        /// <param name="dataId"></param>
        /// <param name="group"></param>
        /// <param name="optional"></param>
        /// <param name="reloadOnChange"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions SubscribeConfiguration(this NacosConfigurationSourceOptions options,
                                                                        string @namespace,
                                                                        string dataId,
                                                                        string group = "DEFAULT_GROUP",
                                                                        bool optional = false,
                                                                        bool reloadOnChange = true)
        {
            options.Subscriptions.Add(new OptionalNacosConfigurationDescriptor(@namespace, dataId, group, optional) { ReloadOnChange = reloadOnChange });
            return options;
        }

        #endregion LoadConfiguration
    }
}