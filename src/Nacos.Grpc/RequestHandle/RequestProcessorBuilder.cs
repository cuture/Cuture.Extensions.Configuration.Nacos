using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using Nacos.Grpc.Messages;

namespace Nacos.Grpc;

/// <summary>
/// 请求处理器构建器
/// </summary>
public class RequestProcessorBuilder
{
    #region Private 字段

    private readonly Dictionary<Type, Func<object, Task<NacosResponse>>> _handleFunctions;

    #endregion Private 字段

    #region Public 构造函数

    /// <summary>
    /// <inheritdoc cref="RequestProcessor"/>
    /// </summary>
    /// <param name="assemblies">自动发现Handler的程序集</param>
    public RequestProcessorBuilder(params Assembly[] assemblies)
    {
        _handleFunctions = assemblies?.Length > 0
                                ? RequestProcessorUtil.CreateHandleFunctions(assemblies)
                                : new();
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 构建请求处理器
    /// </summary>
    /// <returns></returns>
    public IRequestProcessor Build()
    {
        return new RequestProcessor(_handleFunctions);
    }

    /// <summary>
    /// 自动注册程序集中的请求处理器
    /// </summary>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public RequestProcessorBuilder RegisterHandler(Assembly assembly)
    {
        if (assembly is null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var handleFunctions = RequestProcessorUtil.CreateHandleFunctions(assembly);

        foreach (var item in handleFunctions)
        {
            if (_handleFunctions.ContainsKey(item.Key))
            {
                throw new ArgumentException($"已经包含 {item.Key} 的处理器");
            }
            _handleFunctions.Add(item.Key, item.Value);
        }
        return this;
    }

    /// <summary>
    /// 注册请求处理器
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <param name="handler"></param>
    /// <returns></returns>
    public RequestProcessorBuilder RegisterHandler<TRequest>(IRequestHandler<TRequest> handler) where TRequest : NacosRequest
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var type = typeof(TRequest);
        if (_handleFunctions.ContainsKey(type))
        {
            throw new ArgumentException($"已经包含 {type.FullName} 的处理器");
        }

        _handleFunctions.Add(type, async request =>
        {
            var typedRequest = request as TRequest;
            return await handler.HandleAsync(typedRequest!).ConfigureAwait(false);
        });

        return this;
    }

    #endregion Public 方法
}
