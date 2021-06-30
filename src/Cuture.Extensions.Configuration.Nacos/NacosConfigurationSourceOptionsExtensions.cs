using System;
using System.Collections.Generic;
using System.Linq;

using Cuture.Extensions.Configuration.Nacos;

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
            IServerAddressAccessor serverAddressAccessor = options.Servers.TryGetEndpointServerUris(out var serverUris)
                                                                ? new RemoteServerAddressAccessor(serverUris.First().HttpUri, options.LoggerFactory?.CreateLogger<RemoteServerAddressAccessor>())
                                                                : new FixedServerAddressAccessor(options.Servers.ToArray());

            var clientOptions = new NacosHttpConfigurationClientOptions($"NacosHttpConfigurationClient-{Guid.NewGuid():n}", serverAddressAccessor)
            {
                LoggerFactory = options.LoggerFactory,
                User = options.User,
                AcsProfile = options.AcsProfile,
            };

            clientOptions.ConfigurationClientMiddlewares.AddRange(options.ConfigurationClientMiddlewares);

            return new NacosConfigurationHttpClient(clientOptions);
        }

        #endregion HttpClient

        #region Public 方法

        #region AddServerAddress

        /// <summary>
        /// 添加服务地址
        /// </summary>
        /// <param name="options"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions AddServerAddress(this NacosConfigurationSourceOptions options, string url)
        {
            return options.AddServerAddress(ServerUri.Parse(url));
        }

        /// <summary>
        /// 添加服务地址
        /// </summary>
        /// <param name="options"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions AddServerAddress(this NacosConfigurationSourceOptions options, Uri uri)
        {
            return options.AddServerAddress(ServerUri.Parse(uri));
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
        /// <param name="urls"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions AddServerAddress(this NacosConfigurationSourceOptions options, IEnumerable<string> urls)
        {
            return options.AddServerAddress(urls.Select(ServerUri.Parse));
        }

        /// <summary>
        /// 添加服务地址
        /// </summary>
        /// <param name="options"></param>
        /// <param name="uris"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions AddServerAddress(this NacosConfigurationSourceOptions options, IEnumerable<Uri> uris)
        {
            return options.AddServerAddress(uris.Select(ServerUri.Parse));
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

        #endregion AddServerAddress

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
        /// 使用 阿里云ACS
        /// </summary>
        /// <param name="options"></param>
        /// <param name="regionId"></param>
        /// <param name="accessKeyId"></param>
        /// <param name="accessKeySecret"></param>
        /// <returns></returns>
        public static NacosConfigurationSourceOptions WithAliyunACS(this NacosConfigurationSourceOptions options, string regionId, string accessKeyId, string accessKeySecret)
        {
            options.AcsProfile = new(regionId, accessKeyId, accessKeySecret);
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

        #endregion Public 方法

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