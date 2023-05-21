using System;
using Cuture.Extensions.Configuration.Nacos;
using Nacos;

namespace Microsoft.Extensions.Configuration;

/// <summary>
///
/// </summary>
public static class NacosConfigurationBuilderExtensions
{
    /// <summary>
    /// 添加Nacos，并允许使用Grpc客户端（仅当使用 <see cref="IConfiguration"/> 配置时有效）
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <param name="setupAction"></param>
    /// <returns></returns>
    public static INacosConfigurationBuilder AddNacosWithGrpcClientAllowed(this IConfigurationBuilder builder, IConfiguration configuration, Action<NacosConfigurationSourceOptions>? setupAction = null)
    {
        return builder.AddNacos(configuration, options =>
        {
            if (configuration.GetSection("ClientType") is IConfigurationSection clientTypeSection
                && clientTypeSection.Value is string type
                && string.Equals("grpc", type, StringComparison.OrdinalIgnoreCase))
            {
                options.UseGrpcClient();
            }

            setupAction?.Invoke(options);
        });
    }
}
