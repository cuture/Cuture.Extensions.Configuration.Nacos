using System;
using System.Threading.Tasks;

namespace Nacos.Middleware;

/// <summary>
/// Nacos配置中间件
/// </summary>
public interface INacosConfigurationClientMiddleware
{
    #region Public 方法

    /// <summary>
    /// 执行Nacos配置获取中间件
    /// </summary>
    /// <param name="context">配置获取上下文</param>
    /// <param name="next">执行后续中间件委托</param>
    /// <returns></returns>
    Task<NacosConfigurationDescriptor> InvokeGetConfigurationAsync(ConfigurationGetContext context,
                                                                   Func<ConfigurationGetContext, Task<NacosConfigurationDescriptor>> next);

    #endregion Public 方法
}
