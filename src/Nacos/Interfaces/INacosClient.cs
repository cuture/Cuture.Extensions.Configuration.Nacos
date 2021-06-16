using System;
using System.Threading.Tasks;

namespace Nacos
{
    /// <summary>
    /// Nacos客户端吧
    /// </summary>
    public interface INacosClient : IDisposable, IAsyncDisposable
    {
        #region Public 属性

        /// <summary>
        /// 客户端类型
        /// </summary>
        NacosClientType Type { get; }

        #endregion Public 属性

        #region Public 方法

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        Task InitAsync();

        #endregion Public 方法
    }
}