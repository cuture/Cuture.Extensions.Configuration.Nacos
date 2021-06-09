using System;
using System.Net;

namespace Nacos
{
    /// <summary>
    /// 宿主地址访问器
    /// </summary>
    public interface IHostAddressAccessor : IDisposable
    {
        /// <summary>
        /// IP地址
        /// </summary>
        IPAddress Address { get; }
    }
}