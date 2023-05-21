using System;
using System.Linq;

using Cuture.Extensions.Configuration.Nacos;

using Microsoft.Extensions.Logging;

using Nacos;
using Nacos.Grpc;

namespace Microsoft.Extensions.Configuration;

/// <summary>
///
/// </summary>
public static partial class NacosConfigurationSourceOptionsExtensions
{
    #region Public 方法

    /// <summary>
    /// 使用Grpc客户端
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public static NacosConfigurationSourceOptions UseGrpcClient(this NacosConfigurationSourceOptions options)
    {
        options.ClientCreationFunction = CreateGrpcConfigurationClient;
        return options;
    }

    #endregion Public 方法

    #region Private 方法

    private static INacosConfigurationClient CreateGrpcConfigurationClient(NacosConfigurationSourceOptions options)
    {
        IServerAddressAccessor serverAddressAccessor = options.Servers.TryGetEndpointServerUris(out var serverUris)
                                                            ? new RemoteServerAddressAccessor(serverUris.First().HttpUri, options.LoggerFactory?.CreateLogger<RemoteServerAddressAccessor>())
                                                            : new FixedServerAddressAccessor(options.Servers.ToArray());

        IHostAddressAccessor hostAddressAccessor = options.SpecifyClientIP is null
                                                        ? new AutomaticHostAddressAccessor(options.ClientIPSubnet)
                                                        : new FixedHostAddressAccessor(options.SpecifyClientIP);

        var clientOptions = new NacosGrpcConfigurationClientOptions(clientName: $"NacosGrpcConfigurationClient-{Guid.NewGuid():n}",
                                                                    serverAddressAccessor: serverAddressAccessor,
                                                                    hostAddressAccessor: hostAddressAccessor)
        {
            LoggerFactory = options.LoggerFactory,
            User = options.User,
            AcsProfile = options.AcsProfile,
        };

        clientOptions.ConfigurationClientMiddlewares.AddRange(options.ConfigurationClientMiddlewares);

        return new NacosConfigurationGrpcClient(clientOptions);
    }

    #endregion Private 方法
}
