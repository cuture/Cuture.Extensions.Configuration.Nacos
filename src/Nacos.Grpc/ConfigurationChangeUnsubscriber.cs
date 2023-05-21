using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nacos.Grpc;

/// <summary>
/// 配置变更订阅的取消器
/// </summary>
internal class ConfigurationChangeUnsubscriber : IAsyncDisposable
{
    #region Private 字段

    private NacosConfigurationDescriptor _descriptor;
    private int _isDisposed = 0;
    private ConfigurationChangeNotifyCallback _notifyCallback;
    private Func<NacosConfigurationDescriptor, ConfigurationChangeNotifyCallback, Task> _unSubscriberAction;

    #endregion Private 字段

    #region Public 构造函数

    public ConfigurationChangeUnsubscriber(NacosConfigurationDescriptor descriptor,
                                           ConfigurationChangeNotifyCallback notifyCallback,
                                           Func<NacosConfigurationDescriptor, ConfigurationChangeNotifyCallback, Task> unSubscriberAction)
    {
        if (descriptor is null)
        {
            throw new ArgumentNullException(nameof(descriptor));
        }

        if (notifyCallback is null)
        {
            throw new ArgumentNullException(nameof(notifyCallback));
        }

        if (unSubscriberAction is null)
        {
            throw new ArgumentNullException(nameof(unSubscriberAction));
        }

        _descriptor = descriptor;
        _notifyCallback = notifyCallback;
        _unSubscriberAction = unSubscriberAction;
    }

    #endregion Public 构造函数

    #region Public 方法

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Increment(ref _isDisposed) == 1)
        {
            var descriptor = _descriptor;
            var notifyCallback = _notifyCallback;
            var unSubscriberAction = _unSubscriberAction;

            _descriptor = null!;
            _notifyCallback = null!;
            _unSubscriberAction = null!;

            await unSubscriberAction(descriptor, notifyCallback).ConfigureAwait(false);
        }
    }

    #endregion Public 方法
}
