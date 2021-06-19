using System;
using System.Threading.Tasks;

namespace Nacos.Middleware
{
    /// <summary>
    /// 工作管道构建器拓展
    /// </summary>
    internal static class WorkPipelineBuilderExtensions
    {
        #region Public 方法

        public static WorkPipelineBuilder<TContext, TResult> Use<TContext, TResult>(this WorkPipelineBuilder<TContext, TResult> pipelineBuilder, Func<TContext, Func<TContext, Task<TResult>>, Task<TResult>> middleware)
        {
            return pipelineBuilder.Use(next => context => middleware(context, next));
        }

        #endregion Public 方法
    }
}