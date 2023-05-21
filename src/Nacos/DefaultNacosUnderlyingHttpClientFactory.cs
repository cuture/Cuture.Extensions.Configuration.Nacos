using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nacos;

/// <inheritdoc cref="INacosUnderlyingHttpClientFactory"/>
public class DefaultNacosUnderlyingHttpClientFactory : INacosUnderlyingHttpClientFactory
{
    #region Private 字段

    private readonly CancellationTokenSource _runningTokenSource = new();
    private bool _disposedValue;
    private CountingNacosHttpClientHandler _httpClientHandler = new();

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 共享的 <see cref="DefaultNacosUnderlyingHttpClientFactory"/>
    /// </summary>
    public static INacosUnderlyingHttpClientFactory Shared => new DefaultNacosUnderlyingHttpClientFactory();

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="DefaultNacosUnderlyingHttpClientFactory"/>
    public DefaultNacosUnderlyingHttpClientFactory()
    {
        var token = _runningTokenSource.Token;
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                //HACK 每四分钟切换一次HttpClientHandler
                await Task.Delay(TimeSpan.FromMinutes(4), token).ConfigureAwait(false);
                Debug.WriteLine("DefaultNacosUnderlyingHttpClientFactory - 替换旧的 HttpClientHandler");
                var newHandler = new CountingNacosHttpClientHandler();
                var oldHandler = Interlocked.Exchange(ref _httpClientHandler, newHandler);
                oldHandler.MakeDisposeReady(true);
            }
        }, token);
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public HttpClient CreateClient(Uri uri)
    {
        if (_disposedValue)
        {
            throw new ObjectDisposedException(nameof(DefaultNacosUnderlyingHttpClientFactory));
        }

        var client = new CountingNacosHttpClient(_httpClientHandler)
        {
            BaseAddress = uri,
            Timeout = Timeout.InfiniteTimeSpan
        };

        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "UTF-8");

        return client;
    }

    #endregion Public 方法

    #region Dispose

    /// <summary>
    ///
    /// </summary>
    ~DefaultNacosUnderlyingHttpClientFactory()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
            _runningTokenSource.SilenceRelease();

            _httpClientHandler.MakeDisposeReady(disposing);
        }
    }

    #endregion Dispose

    #region Private 类

    private class CountingNacosHttpClient : HttpClient
    {
        #region Private 字段

        private readonly CountingNacosHttpClientHandler _handler;

        #endregion Private 字段

        #region Public 构造函数

        public CountingNacosHttpClient(CountingNacosHttpClientHandler handler) : base(handler, false)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _handler.IncrementReference();
        }

        #endregion Public 构造函数

        #region Protected 方法

        protected override void Dispose(bool disposing)
        {
            _handler.DecrementReference();

            base.Dispose(disposing);
        }

        #endregion Protected 方法
    }

    private class CountingNacosHttpClientHandler : HttpClientHandler
    {
        #region Private 字段

        private readonly object _syncRoot = new();
        private bool _disposeReady = false;

        private int _referenceCount = 0;

        #endregion Private 字段

        #region Public 构造函数

        public CountingNacosHttpClientHandler()
        {
            UseCookies = false;
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli;
        }

        #endregion Public 构造函数

        #region Public 方法

        public void DecrementReference()
        {
            Debug.WriteLine("CountingNacosHttpClientHandler - Decrement ReferenceCount");
            lock (_syncRoot)
            {
                if (--_referenceCount == 0
                    && _disposeReady)
                {
                    Debug.WriteLine("CountingNacosHttpClientHandler - Dispose at DecrementReference");
                    Dispose();
                }
            }
        }

        public void IncrementReference()
        {
            lock (_syncRoot)
            {
                if (!_disposeReady)
                {
                    Debug.WriteLine("CountingNacosHttpClientHandler - Increment ReferenceCount");
                    _referenceCount++;
                    return;
                }
            }
            throw new InvalidOperationException("Handler已准备释放，此时不允许新的引用");
        }

        /// <summary>
        /// 设置为可释放，不允许再被引用，并在引用为 0 时释放自身
        /// </summary>
        public void MakeDisposeReady(bool disposing)
        {
            Debug.WriteLine("CountingNacosHttpClientHandler - Ready to Dispose");
            lock (_syncRoot)
            {
                _disposeReady = true;
                if (_referenceCount == 0)
                {
                    Debug.WriteLine("CountingNacosHttpClientHandler - Dispose at MakeDisposeReady");
                    Dispose(disposing);
                }
            }
        }

        #endregion Public 方法
    }

    #endregion Private 类
}
