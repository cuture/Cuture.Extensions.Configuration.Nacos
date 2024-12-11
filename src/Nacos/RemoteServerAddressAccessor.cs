using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nacos.Exceptions;
using Nacos.Internal;
using Nacos.Utils;

namespace Nacos;

/// <summary>
/// 远程 <inheritdoc cref="IServerAddressAccessor"/>
/// </summary>
public sealed class RemoteServerAddressAccessor : IServerAddressAccessor
{
    #region Private 字段

    private readonly INacosUnderlyingHttpClientFactory _httpClientFactory;
    private readonly ILogger? _logger;
    private readonly CancellationTokenSource _runningCts = new();
    private readonly Uri _serverListRequestUri;
    private readonly object _syncRoot = new();
    private ServerUri[]? _allAddress;
    private int? _count;
    private ServerUri? _currentAddress;
    private int _index = 0;
    private bool _isDisposed = false;

    #endregion Private 字段

    #region Public 属性

    /// <inheritdoc/>
    public ServerUri[] AllAddress => ThrowNotInitialization(_allAddress);

    /// <inheritdoc/>
    public int Count => _count ?? throw new NacosException("Not Initialization yet");

    /// <inheritdoc/>
    public ServerUri CurrentAddress => ThrowNotInitialization(_currentAddress);

    /// <inheritdoc/>
    public string Name { get; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="RemoteServerAddressAccessor"/>
    public RemoteServerAddressAccessor(Uri serverListRequestUri, ILogger<RemoteServerAddressAccessor>? logger)
    {
        _serverListRequestUri = serverListRequestUri ?? throw new ArgumentNullException(nameof(serverListRequestUri));
        _logger = logger;

        _httpClientFactory = DefaultNacosUnderlyingHttpClientFactory.Shared;

        Name = $"remote-{serverListRequestUri}";
    }

    #endregion Public 构造函数

    #region Private 析构函数

    /// <summary>
    ///
    /// </summary>
    ~RemoteServerAddressAccessor()
    {
        Dispose();
    }

    #endregion Private 析构函数

    #region Public 方法

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        _runningCts.SilenceRelease();

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public ServerUri GetRandomAddress()
    {
        var allAddress = ThrowNotInitialization(_allAddress);

        if (allAddress.Length == 1)
        {
            return allAddress[0];
        }

        return allAddress[RandomUtil.Random(allAddress.Length)];
    }

    /// <inheritdoc/>
    public async Task InitAsync(CancellationToken token)
    {
        var serverUris = await GetServerUrisFromAcsEndpointAsync(token).ConfigureAwait(false);

        SetNewAddresses(serverUris);

        StartAutoRefreshAddressBackgroundTask();
    }

    /// <inheritdoc/>
    public ServerUri MoveNextAddress()
    {
        var allAddress = ThrowNotInitialization(_allAddress);

        if (allAddress.Length == 1)
        {
            return allAddress[0];
        }

        lock (_syncRoot)
        {
            _index++;
            if (_index >= allAddress.Length)
            {
                _index = 0;
            }
            _currentAddress = allAddress[_index];
            return _currentAddress;
        }
    }

    #endregion Public 方法

    #region Private 方法

    private static T ThrowNotInitialization<T>(T? value)
    {
        if (value is null)
        {
            throw new NacosException("Not Initialization yet");
        }
        return value;
    }

    private async Task<Uri[]> GetServerUrisFromAcsEndpointAsync(CancellationToken token)
    {
        Uri[]? serverUris = null;

        using var client = _httpClientFactory.CreateClient(_serverListRequestUri);

        for (int i = 0; i <= 2; i++)
        {
            try
            {
                var content = await client.GetStringAsync(_serverListRequestUri, token).ConfigureAwait(false);
                var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                serverUris = lines.Select(m =>
                {
                    var uriBuilder = new UriBuilder(m.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? m : $"http://{m}");
                    if (!m.Contains(':'))
                    {
                        uriBuilder.Port = Constants.DEFAULT_HTTP_PORT;
                    }
                    return uriBuilder.Uri;
                }).ToArray();

                break;
            }
            catch (Exception ex)
            {
                token.ThrowIfCancellationRequested();

                if (i == 2)
                {
                    break;
                }
                _logger?.LogError(ex, "Get server address from remote - {Uri} fail. retrying.", _serverListRequestUri);
                await Task.Delay(1000, token).ConfigureAwait(false);
            }
        }

        if (serverUris is null
            || !serverUris.Any())
        {
            throw new NacosException($"Get server address from remote - {_serverListRequestUri} fail.");
        }

        return serverUris;
    }

    private void SetNewAddresses(Uri[]? serverUris)
    {
        if (serverUris is null
            || !serverUris.Any())
        {
            throw new NacosException("Get address from remote fail.");
        }

        var allAddress = serverUris.Select(m => ServerUri.Parse(m)).ToArray();

        lock (_syncRoot)
        {
            _allAddress = allAddress;
            _count = allAddress.Length;
            _index = RandomUtil.Random(allAddress.Length);
            _currentAddress = allAddress[_index];
        }
    }

    private void StartAutoRefreshAddressBackgroundTask()
    {
        var token = _runningCts.Token;
        _ = Task.Run(async () =>
        {
            var scaler = new Scaler(10, 10, 60);

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), token).ConfigureAwait(false);

                try
                {
                    var serverUris = await GetServerUrisFromAcsEndpointAsync(token).ConfigureAwait(false);

                    SetNewAddresses(serverUris);

                    scaler.Reset();
                }
                catch (Exception ex)
                {
                    token.ThrowIfCancellationRequested();

                    scaler.Add();

                    _logger?.LogError(ex, "Remote Address Refresh Fail. Retry after {ScalerValue} s", scaler.Value);

                    await Task.Delay(TimeSpan.FromSeconds(scaler.Value), token).ConfigureAwait(false);
                }
            }
        }, token);
    }

    #endregion Private 方法
}
