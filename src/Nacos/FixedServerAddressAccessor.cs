using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Nacos.Utils;

namespace Nacos
{
    /// <summary>
    /// 固定的 <inheritdoc cref="IServerAddressAccessor"/>
    /// </summary>
    public sealed class FixedServerAddressAccessor : IServerAddressAccessor
    {
        #region Private 字段

        private readonly ServerUri[] _addresses;
        private readonly object _syncRoot = new();
        private int _index = 0;

        #endregion Private 字段

        #region Public 属性

        /// <inheritdoc/>
        public ServerUri[] AllAddress => (ServerUri[])_addresses.Clone();

        /// <inheritdoc/>
        public int Count { get; }

        /// <inheritdoc/>
        public ServerUri CurrentAddress { get; private set; }

        /// <inheritdoc/>
        public string Name { get; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="FixedServerAddressAccessor"/>
        public FixedServerAddressAccessor(params string[] addresses) : this(addresses.Select(ServerUri.Parse).ToArray())
        {
        }

        /// <inheritdoc cref="FixedServerAddressAccessor"/>
        public FixedServerAddressAccessor(params ServerUri[] addresses)
        {
            if (addresses is null)
            {
                throw new ArgumentNullException(nameof(addresses));
            }

            if (addresses.Length < 1)
            {
                throw new ArgumentException("必须具有服务地址", nameof(addresses));
            }

            _addresses = addresses;

            Name = string.Join("-", _addresses.Select(m => $"{m.Host}_{m.HttpPort}_{m.GrpcPort}"));

            _index = RandomUtil.Random(_addresses.Length);

            CurrentAddress = _addresses[_index];

            Count = _addresses.Length;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public ServerUri GetRandomAddress() => _addresses[RandomUtil.Random(_addresses.Length)];

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken token) => Task.CompletedTask;

        /// <inheritdoc/>
        public ServerUri MoveNextAddress()
        {
            if (_addresses.Length == 1)
            {
                return _addresses[0];
            }

            lock (_syncRoot)
            {
                _index++;
                if (_index >= _addresses.Length)
                {
                    _index = 0;
                }
                CurrentAddress = _addresses[_index];
                return CurrentAddress;
            }
        }

        #endregion Public 方法

        #region Dispose

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        #endregion Dispose
    }
}