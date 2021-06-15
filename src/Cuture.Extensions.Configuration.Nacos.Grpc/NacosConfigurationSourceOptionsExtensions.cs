using System;
using Nacos;
using Nacos.Grpc;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    ///
    /// </summary>
    public static partial class NacosConfigurationSourceOptionsExtensions
    {
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

        private static INacosConfigurationClient CreateGrpcConfigurationClient(NacosConfigurationSourceOptions options)
        {
            var serverAddressAccessor = new FixedServerAddressAccessor(options.Servers.ToArray());

            IHostAddressAccessor hostAddressAccessor = options.SpecifyClientIP is null
                                                            ? new AutomaticHostAddressAccessor(options.ClientIPSubnet)
                                                            : new FixedHostAddressAccessor(options.SpecifyClientIP);

            var clientOptions = new NacosGrpcClientOptions(clientName: $"NacosGrpcClient-{Guid.NewGuid():n}",
                                                           serverAddressAccessor: serverAddressAccessor,
                                                           hostAddressAccessor: hostAddressAccessor)
            {
                LoggerFactory = options.LoggerFactory,
                User = options.User,
                AcsProfile = options.AcsProfile,
            };

            return new NacosConfigurationGrpcClient(clientOptions);
        }
    }
}