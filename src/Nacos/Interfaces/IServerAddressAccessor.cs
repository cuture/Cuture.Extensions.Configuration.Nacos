using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nacos
{
    /// <summary>
    /// 服务器地址访问器
    /// </summary>
    public interface IServerAddressAccessor : IDisposable
    {
        #region Public 属性

        /// <summary>
        /// 所有地址
        /// </summary>
        ServerUri[] AllAddress { get; }

        /// <summary>
        /// 总数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 当前使用的地址
        /// </summary>
        ServerUri CurrentAddress { get; }

        /// <summary>
        /// 访问器名称
        /// </summary>
        string Name { get; }

        #endregion Public 属性

        #region Public 方法

        /// <summary>
        /// 获取一个随机地址
        /// </summary>
        /// <returns></returns>
        ServerUri GetRandomAddress();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task InitAsync(CancellationToken token);

        /// <summary>
        /// 切换到下一个地址
        /// </summary>
        /// <returns></returns>
        ServerUri MoveNextAddress();

        #endregion Public 方法
    }
}