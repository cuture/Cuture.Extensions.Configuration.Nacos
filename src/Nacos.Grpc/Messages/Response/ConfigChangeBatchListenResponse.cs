using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Nacos.Grpc.Messages
{
    /// <summary>
    /// 配置批量监听响应
    /// </summary>
    public class ConfigChangeBatchListenResponse : NacosResponse
    {
        #region Public 字段

        /// <summary>
        /// 有变更的配置
        /// </summary>
        [JsonPropertyName("changedConfigs")]
        public List<ConfigContext> ChangedConfigs = new();

        #endregion Public 字段

        #region Public 类

        /// <summary>
        /// 配置内容
        /// </summary>
        public class ConfigContext
        {
            #region Public 属性

            /// <summary>
            ///
            /// </summary>
            [JsonPropertyName("dataId")]
            public string? DataId { get; set; }

            /// <summary>
            ///
            /// </summary>
            [JsonPropertyName("group")]
            public string? Group { get; set; }

            /// <summary>
            ///
            /// </summary>
            [JsonPropertyName("tenant")]
            public string? Namespace { get; set; }

            #endregion Public 属性
        }

        #endregion Public 类
    }
}