using System;
using System.Text.Json.Serialization;

using Nacos.Utils;

namespace Nacos.Grpc.Messages
{
    /// <summary>
    /// Nacos请求
    /// </summary>
    public abstract class NacosRequest
    {
        #region Public 属性

        /// <summary>
        /// 请求头
        /// </summary>
        [JsonIgnore]
        public NacosHeaders Headers { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="NacosRequest"/>
        public NacosRequest() : this(true)
        {
        }

        /// <summary>
        /// <inheritdoc cref="NacosRequest"/>
        /// </summary>
        /// <param name="setGenericHeaders">是否设置通用头部</param>
        public NacosRequest(bool setGenericHeaders = false)
        {
            Headers = new(setGenericHeaders);
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <summary>
        /// 签名请求
        /// </summary>
        /// <param name="appKey"></param>
        /// <returns></returns>
        public NacosRequest Sign(string? appKey = null)
        {
            var timestamp = Headers.ClientRequestTimestamp;
            if (timestamp is null)
            {
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
                Headers.ClientRequestTimestamp = timestamp;
                Headers.Timestamp = timestamp;
            }

            Headers.ClientRequestToken = HashUtil.GetMd5(timestamp + (string.IsNullOrWhiteSpace(appKey) ? "" : appKey));

            return this;
        }

        #endregion Public 方法
    }
}