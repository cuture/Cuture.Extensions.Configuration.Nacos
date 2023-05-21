using System;
using System.Net;
using Nacos.Utils;

namespace Nacos;

/// <summary>
/// 自动获取地址的 <inheritdoc cref="IHostAddressAccessor"/>
/// </summary>
public class AutomaticHostAddressAccessor : IHostAddressAccessor
{
    private readonly string? _subnet;
    private readonly TimeSpan? _validTimeSpan;

    private IPAddress _address;

    private DateTime _lastRefreshAddressTime = DateTime.UtcNow;

    /// <inheritdoc/>
    public IPAddress Address
    {
        get
        {
            if (_validTimeSpan.HasValue
                && DateTime.UtcNow - _lastRefreshAddressTime > _validTimeSpan.Value)
            {
            }

            return _address;
        }
    }

    /// <summary>
    /// <inheritdoc cref="AutomaticHostAddressAccessor"/>
    /// </summary>
    /// <param name="subnet">指定子网<para/>eg:<para/>192.168.0.1/24<para/>10.0.0.1/8</param>
    /// <param name="validTimeSpan">有效时间（重新获取的时长）</param>
    public AutomaticHostAddressAccessor(string? subnet = null, TimeSpan? validTimeSpan = null)
    {
        _subnet = subnet;
        _validTimeSpan = validTimeSpan;
        _address = RefreshAddress();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }

    private IPAddress RefreshAddress()
    {
        _lastRefreshAddressTime = DateTime.UtcNow;
        return _address = NetworkUtil.GetValuableIPAddress(_subnet);
    }
}
