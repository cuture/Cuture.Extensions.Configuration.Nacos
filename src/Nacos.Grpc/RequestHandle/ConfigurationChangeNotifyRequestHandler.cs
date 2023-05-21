using System;
using System.Threading.Tasks;

using Nacos.Grpc.Messages;

namespace Nacos.Grpc;

internal class ConfigurationChangeNotifyRequestHandler
    : IRequestHandler<ConfigChangeNotifyRequest>
{
    #region Private 字段

    private readonly Func<ConfigChangeNotifyRequest, Task> _notifyCallback;

    #endregion Private 字段

    #region Public 构造函数

    public ConfigurationChangeNotifyRequestHandler(Func<ConfigChangeNotifyRequest, Task> notifyCallback)
    {
        _notifyCallback = notifyCallback ?? throw new ArgumentNullException(nameof(notifyCallback));
    }

    #endregion Public 构造函数

    #region Public 方法

    public async Task<NacosResponse> HandleAsync(ConfigChangeNotifyRequest request)
    {
        await _notifyCallback(request).ConfigureAwait(false);
        return new ConfigChangeNotifyResponse();
    }

    #endregion Public 方法
}
