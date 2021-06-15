using System.Threading.Tasks;

using Nacos.Messages;

namespace Nacos
{
    /// <summary>
    /// 空的 <see cref="INacosRequestSigner"/>
    /// </summary>
    public class NullRequestSigner : INacosRequestSigner
    {
        #region Public 方法

        /// <inheritdoc/>
        public Task SignAsync(INacosRequest request) => Task.CompletedTask;

        #endregion Public 方法
    }
}