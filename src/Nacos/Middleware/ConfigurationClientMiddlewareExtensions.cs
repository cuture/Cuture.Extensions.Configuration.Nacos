using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Nacos.Middleware
{
    /// <summary>
    /// Nacos配置中间件拓展
    /// </summary>
    public static class ConfigurationClientMiddlewareExtensions
    {
        #region Public 方法

        /// <summary>
        /// 构建获取配置的委托
        /// </summary>
        /// <param name="middlewares"></param>
        /// <param name="endpointDelegate">最终获取配置的方法委托</param>
        /// <returns></returns>
        public static Func<ConfigurationGetContext, Task<NacosConfigurationDescriptor>> BuildGetConfigurationDelegate(this IEnumerable<INacosConfigurationClientMiddleware> middlewares,
                                                                                                                          Func<ConfigurationGetContext, Task<NacosConfigurationDescriptor>> endpointDelegate)
        {
            if (!middlewares.Any())
            {
                return endpointDelegate;
            }

            var pipelineBuilder = new WorkPipelineBuilder<ConfigurationGetContext, NacosConfigurationDescriptor>(endpointDelegate);

            foreach (var middleware in middlewares)
            {
                pipelineBuilder.Use(middleware.InvokeGetConfigurationAsync);
            }

            return pipelineBuilder.Build();
        }

        /// <summary>
        /// 构建获取配置的委托，包含阿里云KMS解密（如果可用）
        /// </summary>
        /// <param name="middlewares"></param>
        /// <param name="acsProfile"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="httpClientFactory"></param>
        /// <param name="endpointDelegate">最终获取配置的方法委托</param>
        /// <returns></returns>
        public static Func<ConfigurationGetContext, Task<NacosConfigurationDescriptor>> BuildGetConfigurationDelegateWithAliyunKMSDecrypt(this IEnumerable<INacosConfigurationClientMiddleware> middlewares,
                                                                                                                                          AliyunAcsProfile? acsProfile,
                                                                                                                                          ILoggerFactory? loggerFactory,
                                                                                                                                          INacosUnderlyingHttpClientFactory? httpClientFactory,
                                                                                                                                          Func<ConfigurationGetContext, Task<NacosConfigurationDescriptor>> endpointDelegate)
        {
            if (acsProfile is not null)
            {
                var aliyunKMSConfigurationClientMiddleware = new AliyunKMSConfigurationClientMiddleware(acsProfile: acsProfile,
                                                                                                        logger: loggerFactory?.CreateLogger<AliyunKMSConfigurationClientMiddleware>(),
                                                                                                        httpClientFactory: httpClientFactory);
                middlewares = middlewares.Append(aliyunKMSConfigurationClientMiddleware);
            }
            return middlewares.BuildGetConfigurationDelegate(endpointDelegate);
        }

        #endregion Public 方法
    }
}