using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Cuture.Extensions.Configuration.Nacos;

using Microsoft.Extensions.Logging;

using Nacos;
using Nacos.Exceptions;

namespace Microsoft.Extensions.Configuration;

/// <summary>
/// Nacos配置拓展
/// </summary>
public static class NacosConfigurationBuilderExtensions
{
    #region Public 字段

    /// <summary>
    /// Nacos client Key
    /// </summary>
    public const string NACOS_CLIENT_KEY = "NACOS_CLIENT";

    /// <summary>
    /// loggerfactory Key
    /// </summary>
    public const string NACOS_LOGGER_FACTORY_KEY = "NACOS_LOGGER_FACTORY";

    #endregion Public 字段

    #region Public 方法

    /// <summary>
    /// 添加Nacos配置源
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <param name="setupAction"></param>
    public static INacosConfigurationBuilder AddNacos(this IConfigurationBuilder builder, IConfiguration configuration, Action<NacosConfigurationSourceOptions>? setupAction = null)
    {
        if (configuration is null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        //HACK 目前不引入 Options 库，手动进行绑定

        return builder.InternalAddNacos(options =>
        {
            ConfigurationServers(options, configuration);

            ConfigurationUser(options, configuration);

            ConfigurationNacosConfigurationSubscribe(options, configuration.GetRequiredSection("Configuration"));

            if (configuration.TryGetSection("HealthCheckInterval", out var healthCheckIntervalSection))
            {
                if (TimeSpan.TryParse(healthCheckIntervalSection.Value, out var healthCheckInterval))
                {
                    options.HealthCheckInterval = healthCheckInterval;
                }
                else
                {
                    throw new ArgumentException($"无法将 {healthCheckIntervalSection.Value} 转换为 TimeSpan");
                }
            }

            if (configuration.TryGetSection("ClientIPSubnet", out var clientIPSubnetSection))
            {
                options.ClientIPSubnet = clientIPSubnetSection.Value;
            }

            if (configuration.TryGetSection("SpecifyClientIP", out var specifyClientIPSection))
            {
                if (IPAddress.TryParse(specifyClientIPSection.Value, out var specifyClientIP))
                {
                    options.SpecifyClientIP = specifyClientIP;
                }
                else
                {
                    throw new ArgumentException($"无法将 {specifyClientIPSection.Value} 转换为 IPAddress");
                }
            }

            setupAction?.Invoke(options);
        });
    }

    /// <summary>
    /// 添加Nacos配置源
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="setupAction"></param>
    public static INacosConfigurationBuilder AddNacos(this IConfigurationBuilder builder, Action<NacosConfigurationSourceOptions> setupAction)
    {
        return builder.InternalAddNacos(setupAction, null);
    }

    /// <summary>
    /// 使用之前构建的Nacos客户端，继续添加配置
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static INacosConfigurationBuilder AppendNacos(this INacosConfigurationBuilder builder, OptionalNacosConfigurationDescriptor descriptor)
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        builder.Add(CreateNacosConfigurationSource(builder.Client, descriptor, builder.ConfigurationParsers, TryGetLoggerFactory(builder)));

        return builder;
    }

    /// <summary>
    /// 使用之前构建的Nacos客户端，继续添加配置
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="namespace"></param>
    /// <param name="dataId"></param>
    /// <param name="group"></param>
    /// <param name="optional"></param>
    /// <returns></returns>
    public static INacosConfigurationBuilder AppendNacos(this INacosConfigurationBuilder builder, string @namespace, string dataId, string group = Constants.DEFAULT_GROUP, bool optional = false)
    {
        var descriptor = new OptionalNacosConfigurationDescriptor(@namespace, dataId, group, optional);

        builder.Add(CreateNacosConfigurationSource(builder.Client, descriptor, builder.ConfigurationParsers, TryGetLoggerFactory(builder)));

        return builder;
    }

    #endregion Public 方法

    #region Private 方法

    #region 使用IConfiguration配置Nacos

    private static void ConfigurationNacosConfigurationSubscribe(NacosConfigurationSourceOptions options, IConfiguration configuration)
    {
        var defaultNamespace = configuration.GetValue<string?>("DefaultNamespace");
        var defaultGroup = configuration.GetValue<string?>("DefaultGroup");

        var subscriptionsSection = configuration.GetRequiredSection("Subscriptions");

        var subscriptions = subscriptionsSection.GetChildren().ToList();

        if (subscriptions is null
            || subscriptions.Count == 0)
        {
            throw new ArgumentException("必须在 Configuration:Subscriptions 配置节点正确配置需要订阅的Nacos配置信息");
        }

        foreach (var subscription in subscriptions)
        {
            var @namespace = subscription.GetValue<string>("Namespace") ?? defaultNamespace ?? throw new ArgumentException($"配置订阅 {subscription.Path} 必须设置 Namespace");
            var group = subscription.GetValue<string>("Group") ?? defaultGroup ?? Constants.DEFAULT_GROUP;
            var dataId = subscription.GetValue<string>("DataId") ?? throw new ArgumentException($"配置订阅 {subscription.Path} 必须设置 DataId");
            var optional = subscription.GetValue<bool?>("Optional");
            var reloadOnChange = subscription.GetValue<bool?>("ReloadOnChange");

            options.SubscribeConfiguration(@namespace, dataId, group, optional ?? false, reloadOnChange ?? true);
        }
    }

    private static void ConfigurationServers(NacosConfigurationSourceOptions options, IConfiguration configuration)
    {
        var serversSection = configuration.GetRequiredSection("Servers");

        var serverUriList = serversSection.Get<List<string>>();
        if (serverUriList is null
            || serverUriList.Count == 0)
        {
            throw new ArgumentException("必须在 Servers 配置节点正确配置Nacos地址");
        }

        var servers = serverUriList.Select(m =>
        {
            try
            {
                return ServerUri.Parse(m);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"无法解析配置的Nacos地址 - {m}", ex);
            }
        }).ToArray();

        options.AddServerAddress(servers);
    }

    private static void ConfigurationUser(NacosConfigurationSourceOptions options, IConfiguration configuration)
    {
        if (options.Servers.TryGetEndpointServerUris(out var endpointUris))
        {
            if (endpointUris.Any(m => m.IsAliyunAcm())) //ACM
            {
                if (configuration.TryGetSection("Auth:ACS", out var acsSection))   //阿里云ACS认证信息
                {
                    if (acsSection.TryGetSection("RegionId", out var regionIdSection)
                        && regionIdSection.Value is not null
                        && acsSection.TryGetSection("AccessKeyId", out var accessKeyIdSection)
                        && accessKeyIdSection.Value is not null
                        && acsSection.TryGetSection("AccessKeySecret", out var accessKeySecretSection)
                        && accessKeySecretSection.Value is not null)
                    {
                        options.WithAliyunACS(regionIdSection.Value, accessKeyIdSection.Value, accessKeySecretSection.Value);
                        return;
                    }
                }
                throw new ArgumentException("Auth:ACS 未正确设置");
            }
            //其他情况为普通endpoint，仍然使用nacos账号进行登录
        }

        if (configuration.TryGetSection("Auth:User", out var userSection))  //Nacos登录信息
        {
            if (userSection.TryGetSection("Account", out var accountSection)
                ^ userSection.TryGetSection("Password", out var passwordSection))
            {
                throw new ArgumentException("Auth:User:Account 和 Auth:User:Password 必须同时设置");
            }
            if (accountSection?.Value is not null
                && passwordSection?.Value is not null)
            {
                options.WithUser(accountSection.Value, passwordSection.Value);
            }
        }
    }

    #endregion 使用IConfiguration配置Nacos

    private static NacosConfigurationSource CreateNacosConfigurationSource(INacosConfigurationClient client, OptionalNacosConfigurationDescriptor descriptor, IEnumerable<IConfigurationParser> parsers, ILoggerFactory? loggerFactory = null)
    {
        var parsersCopy = parsers.ToArray();
        return new NacosConfigurationSource(new NacosConfigurationProviderOptions(client, descriptor, parsersCopy, loggerFactory));
    }

    private static INacosConfigurationBuilder InternalAddNacos(this IConfigurationBuilder builder, Action<NacosConfigurationSourceOptions> setupAction, IConfiguration? configuration = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        if (setupAction is null)
        {
            throw new ArgumentNullException(nameof(setupAction));
        }

        var options = new NacosConfigurationSourceOptions();

        setupAction(options);

        if (options.ConfigurationParsers is null
            || options.ConfigurationParsers.Count == 0)
        {
            throw new NacosException($"必须对 {nameof(NacosConfigurationSourceOptions.ConfigurationParsers)} 属性配置有效的解析器");
        }

        var parsers = options.ConfigurationParsers.ToArray();

        INacosConfigurationClient client = options.ClientCreationFunction is not null
                                                ? options.ClientCreationFunction(options)
                                                : NacosConfigurationSourceOptionsExtensions.CreateHttpConfigurationClient(options); //不设置时使用Http

        if (client is null)
        {
            throw new NacosException("未正确创建Nacos客户端");
        }

        client.InitAsync().WaitWithoutContext();

        options.Subscriptions.Select(descriptor => CreateNacosConfigurationSource(client, descriptor, parsers, options.LoggerFactory))
                             .ToList()
                             .ForEach(source =>
                             {
                                 builder.Add(source);
                             });

        builder.Properties[NACOS_CLIENT_KEY] = client;
        builder.Properties[NACOS_LOGGER_FACTORY_KEY] = options.LoggerFactory!;

        return new NacosConfigurationBuilder(builder, configuration, client, parsers);
    }

    private static ILoggerFactory? TryGetLoggerFactory(INacosConfigurationBuilder builder)
    {
        if (builder.Properties.TryGetValue(NACOS_LOGGER_FACTORY_KEY, out var storedLoggerFactory))
        {
            return storedLoggerFactory as ILoggerFactory;
        }

        return null;
    }

    private static bool TryGetSection(this IConfiguration configuration, string sectionName, [NotNullWhen(true)] out IConfigurationSection? configurationSection)
    {
        if (configuration.GetSection(sectionName) is IConfigurationSection section && section.Exists())
        {
            configurationSection = section;
            return true;
        }

        configurationSection = null;
        return false;
    }

    #endregion Private 方法
}
