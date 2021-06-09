using System.Threading;
using System.Threading.Tasks;

namespace Nacos
{
    /// <summary>
    /// 配置变更回调委托
    /// </summary>
    /// <param name="descriptor">新的配置内容</param>
    /// <param name="token"></param>
    /// <returns></returns>
    public delegate Task ConfigurationChangeNotifyCallback(NacosConfigurationDescriptor descriptor, CancellationToken token);
}