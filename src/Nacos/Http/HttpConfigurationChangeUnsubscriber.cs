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

        private CancellationTokenSource? _tokenSource;

        #endregion Private 字段

        #region Public 构造函数

        public HttpConfigurationChangeUnsubscriber(CancellationTokenSource tokenSource)
        {
            _tokenSource = tokenSource ?? throw new ArgumentNullException(nameof(tokenSource));
        }

        #endregion Public 构造函数

        #region Public 方法

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _tokenSource, null!) is CancellationTokenSource tokenSource)
            {
                tokenSource.SilenceRelease();
            }
            return ValueTask.CompletedTask;
        }

        #endregion Public 方法
    }
}