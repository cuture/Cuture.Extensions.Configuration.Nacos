using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Nacos.Grpc.GrpcService;
using Nacos.Grpc.Messages;

namespace Nacos.Grpc
{
    /// <inheritdoc cref="IMessageSerializer"/>
    public sealed class MessageSerializer : IMessageSerializer
    {
        #region Private 字段

        private readonly IReadOnlyDictionary<string, System.Type> _messageTypeMap;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// 单例
        /// </summary>
        public static MessageSerializer Instance { get; } = new();

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="MessageSerializer"/>
        public MessageSerializer(IReadOnlyDictionary<string, System.Type>? messageTypeMap = null)
        {
            _messageTypeMap = messageTypeMap ?? Assembly.GetExecutingAssembly()
                                                        .GetTypes()
                                                        .Where(m => m.IsAssignableTo(typeof(NacosRequest)) || m.IsAssignableTo(typeof(NacosResponse)))
                                                        .ToDictionary(m => m.Name);
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public object Deserialize(Payload payload)
        {
            if (_messageTypeMap.TryGetValue(payload.Metadata.Type, out var type))
            {
                var body = payload.Body.Value.ToStringUtf8();
                return JsonSerializer.Deserialize(body, type) ?? throw new UnknownMessageException($"解析消息体失败：{body}");
            }

            throw new UnknownMessageException(payload.Metadata.Type);
        }

        /// <inheritdoc/>
        public Payload Serialize(NacosRequest request)
        {
            var type = request.GetType();
            var mateData = new Metadata()
            {
                Type = type.Name,
            };

            mateData.Headers.Add(request.Headers);

            SetRequestId(mateData, request.RequestId);

            var payload = new Payload()
            {
                Body = new Any() { Value = ByteString.CopyFromUtf8(JsonSerializer.Serialize(request, type)) },
                Metadata = mateData
            };

            return payload;
        }

        /// <inheritdoc/>
        public Payload Serialize(NacosResponse response)
        {
            var type = response.GetType();
            var mateData = new Metadata()
            {
                Type = type.Name,
            };

            SetRequestId(mateData, response.RequestId);

            var payload = new Payload()
            {
                Body = new Any() { Value = ByteString.CopyFromUtf8(JsonSerializer.Serialize(response, type)) },
                Metadata = mateData
            };

            return payload;
        }

        private static void SetRequestId(Metadata mateData, string? requestId)
        {
            if (requestId is not null)
            {
                mateData.Headers.Add(Constants.Headers.REQUEST_ID, requestId);
            }
        }

        #endregion Public 方法
    }
}