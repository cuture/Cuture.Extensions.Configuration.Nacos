using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nacos.Middleware;

/// <summary>
/// 工作管道构建器
/// </summary>
/// <typeparam name="TContext"></typeparam>
/// <typeparam name="TResult"></typeparam>
internal class WorkPipelineBuilder<TContext, TResult>
{
    #region Private 字段

    private readonly Func<TContext, Task<TResult>> _endpointDelegate;
    private readonly List<Func<Func<TContext, Task<TResult>>, Func<TContext, Task<TResult>>>> _pipelineBuildDelegates = new();

    #endregion Private 字段

    #region Public 属性

    public IDictionary<string, object?> Properties { get; protected set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="WorkPipelineBuilder{TContext, TResult}"/>
    /// </summary>
    /// <param name="endpointDelegate">终结点委托</param>
    public WorkPipelineBuilder(Func<TContext, Task<TResult>> endpointDelegate)
    {
        _endpointDelegate = endpointDelegate ?? throw new ArgumentNullException(nameof(endpointDelegate));

        Properties = new Dictionary<string, object?>(StringComparer.Ordinal);
    }

    #endregion Public 构造函数

    #region Public 方法

    public virtual Func<TContext, Task<TResult>> Build()
    {
        var targetDelegate = _endpointDelegate;

        for (int i = _pipelineBuildDelegates.Count - 1; i >= 0; i--)
        {
            targetDelegate = _pipelineBuildDelegates[i](targetDelegate);
        }

        return targetDelegate;
    }

    public virtual WorkPipelineBuilder<TContext, TResult> Use(Func<Func<TContext, Task<TResult>>, Func<TContext, Task<TResult>>> pipelineBuildDelegate)
    {
        _pipelineBuildDelegates.Add(pipelineBuildDelegate);
        return this;
    }

    #endregion Public 方法
}
