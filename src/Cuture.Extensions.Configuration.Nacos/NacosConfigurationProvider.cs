using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Nacos;
using Nacos.Exceptions;
using Nacos.Utils;

namespace Cuture.Extensions.Configuration.Nacos;

/// <summary>
/// nacos配置提供器
/// </summary>
internal class NacosConfigurationProvider : ConfigurationProvider, IDisposable
{
    #region Private 字段

    private readonly INacosConfigurationClient _client;
    private readonly OptionalNacosConfigurationDescriptor _descriptor;

    private readonly ILogger? _logger;
    private string? _content;
    private bool _disposedValue;
    private IEnumerable<IConfigurationParser> _parsers;
    private IAsyncDisposable? _subscribeDisposer;

    #endregion Private 字段

    #region Public 构造函数

    public NacosConfigurationProvider(NacosConfigurationProviderOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _client = options.Client;
        _descriptor = options.Descriptor;
        _parsers = options.Parsers;
        _logger = options.LoggerFactory?.CreateLogger<NacosConfigurationProvider>();
    }

    #endregion Public 构造函数

    #region Public 方法

    public override void Load()
    {
        var content = _content;
        if (content is null)
        {
            _logger?.LogDebug("Load - 强制同步获取配置 - {0}", _descriptor);
            try
            {
                var configuration = GetConfigurationAsync().WaitWithoutContext();
                content = configuration.Content;
            }
            catch (ConfigurationNotFoundException)
            {
                if (_descriptor.Optional)
                {
                    _logger?.LogDebug("Load - 没有找到配置 - {0}", _descriptor);
                    LoadConfiguration(null);
                    return;
                }
                throw;
            }
        }
        LoadConfiguration(content);
    }

    public override string ToString() => $"NacosConfigurationProvider for {_descriptor}";

    #endregion Public 方法

    #region Internal 方法

    /// <summary>
    /// 初始化
    /// </summary>
    /// <returns></returns>
    internal async Task InitAsync()
    {
        _logger?.LogDebug("开始初始化 - {0}", this);

        using var initCTS = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        var initToken = initCTS.Token;
        try
        {
            try
            {
                var configuration = await GetConfigurationAsync(initToken).ConfigureAwait(false);

                _content = configuration.Content;

                _logger?.LogDebug("{0} 配置获取成功 - {1}", this, _content);
            }
            catch (ConfigurationNotFoundException)
            {
                if (!_descriptor.Optional)
                {
                    throw;
                }
            }

            _logger?.LogDebug("开始订阅配置变更 - {0}", this);

            var subscribeDescriptor = _descriptor.WithContent(_content, HashUtil.ComputeMD5(_content).ToHexString());

            _subscribeDisposer = await _client.SubscribeConfigurationChangeAsync(subscribeDescriptor, OnConfigurationChangeAsync, initToken).ConfigureAwait(false);

            _logger?.LogDebug("配置变更订阅完成 - {0}", this);
        }
        catch (OperationCanceledException)
        {
            if (initToken.IsCancellationRequested)
            {
                _logger?.LogError("获取配置超时 - {0}", this);
                throw new RequestTimeoutException($"获取配置超时 - {_descriptor}");
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "配置提供器初始化异常 - {0}", this);

            throw new NacosException($"配置提供器初始化异常 - {_descriptor}", ex);
        }
    }

    #endregion Internal 方法

    #region Private 方法

    private async Task<NacosConfigurationDescriptor> GetConfigurationAsync(CancellationToken token = default)
    {
        return await _client.GetConfigurationAsync(_descriptor, token).ConfigureAwait(false);
    }

    private void LoadConfiguration(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            Data.Clear();
            return;
        }

        //HACK 异常处理？

        foreach (var parser in _parsers)
        {
            if (parser.CanParse(content))
            {
                Data = parser.Parse(content);
                break;
            }
        }
    }

    private Task OnConfigurationChangeAsync(NacosConfigurationDescriptor descriptor, CancellationToken token)
    {
        try
        {
            _content = descriptor.Content;
            LoadConfiguration(descriptor.Content);

            OnReload();
        }
        catch (Exception ex)
        {
            token.ThrowIfCancellationRequested();

            _logger?.LogError(ex, "加载变更配置失败 {0}", _descriptor);
        }

        return Task.CompletedTask;
    }

    #endregion Private 方法

    #region Dispose

    ~NacosConfigurationProvider()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
            _content = null;
            _parsers = null!;
            var subscribeDisposer = Interlocked.Exchange(ref _subscribeDisposer, null);
            if (subscribeDisposer is not null)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await subscribeDisposer.DisposeAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "释放配置变更订阅异常 - {0}", _descriptor);
                    }
                });
            }
        }
    }

    #endregion Dispose
}
