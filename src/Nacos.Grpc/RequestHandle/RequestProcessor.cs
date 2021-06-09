using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Nacos.Grpc.Messages;

namespace Nacos.Grpc
{
    /// <summary>
    /// 请求处理器
    /// </summary>
    internal class RequestProcessor : IRequestProcessor
    {
        #region Private 字段

        private readonly IReadOnlyDictionary<Type, Func<object, Task<NacosResponse>>> _handleFunctions;

        #endregion Private 字段

        #region Public 构造函数

        /// <summary>
        /// <inheritdoc cref="RequestProcessor"/>
        /// </summary>
        /// <param name="handleFunctions">处理委托字典</param>
        public RequestProcessor(IReadOnlyDictionary<Type, Func<object, Task<NacosResponse>>> handleFunctions)
        {
            _handleFunctions = handleFunctions ?? throw new ArgumentNullException(nameof(handleFunctions));
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <summary>
        /// 处理请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<NacosResponse> HandleAsync(NacosRequest request)
        {
            if (_handleFunctions.TryGetValue(request.GetType(), out var handleFunction))
            {
                return handleFunction(request);
            }
            throw new UnknownMessageException($"未知的处理进程消息 - {request}");
        }

        #endregion Public 方法
    }
}