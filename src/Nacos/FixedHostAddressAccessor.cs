using System;
using System.Net;

namespace Nacos
{
    /// <summary>
    /// 固定的 <inheritdoc cref="IHostAddressAccessor"/>
    /// </summary>
    public class FixedHostAddressAccessor : IHostAddressAccessor
    {
        /// <inheritdoc/>
        public IPAddress Address { get; }

        /// <inheritdoc cref="FixedHostAddressAccessor"/>
        public FixedHostAddressAccessor(IPAddress address)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}