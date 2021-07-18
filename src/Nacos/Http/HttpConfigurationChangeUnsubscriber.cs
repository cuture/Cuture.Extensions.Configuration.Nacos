using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nacos.Http
{
    /// <summary>
    /// Http配置变更订阅的取消器
    /// </summary>
    internal class HttpConfigurationChangeUnsubscriber : IAsyncDisposable
    {
        #region Private 字段

        private NacosConfigurationDescriptor _descriptor;

        private int _isDisposed = 0;

        private ConfigurationChangeNotifyCallback _notifyCallback;

        private Action<NacosConfigurationDescriptor, ConfigurationChangeNotifyCallback> _unSubscriberAction;

        #endregion Private 字段

        #region Public 构造函数

        public HttpConfigurationChangeUnsubscriber(NacosConfigurationDescriptor descriptor,
                                                   ConfigurationChangeNotifyCallback notifyCallback,
                                                   Action<NacosConfigurationDescriptor, ConfigurationChangeNotifyCallback> unSubscriberAction)
        {
            _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
            _notifyCallback = notifyCallback ?? throw new ArgumentNullException(nameof(notifyCallback));
            _unSubscriberAction = unSubscriberAction ?? throw new ArgumentNullException(nameof(unSubscriberAction));
        }

        #endregion Public 构造函数

        #region Public 方法

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Increment(ref _isDisposed) == 1)
            {
                var descriptor = _descriptor;
                var notifyCallback = _notifyCallback;
                var unSubscriberAction = _unSubscriberAction;

                _descriptor = null!;
                _notifyCallback = null!;
                _unSubscriberAction = null!;

                unSubscriberAction(descriptor, notifyCallback);
            }
            return ValueTask.CompletedTask;
        }

        #endregion Public 方法
    }
}