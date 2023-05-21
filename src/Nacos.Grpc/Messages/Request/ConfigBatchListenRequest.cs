using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释

namespace Nacos.Grpc.Messages;

/// <summary>
/// 配置变更批量监听请求
/// </summary>
public class ConfigBatchListenRequest : NacosRequest
{
    #region Private 字段

    private readonly List<ConfigListenContext> _configListenContexts = new();

    #endregion Private 字段

    #region Public 属性

    /// <summary>
    /// 监听或取消监听
    /// </summary>
    [JsonPropertyName("listen")]
    public bool IsListen { get; set; } = true;

    /// <summary>
    /// 监听内容
    /// </summary>
    [JsonPropertyName("configListenContexts")]
    public IReadOnlyList<ConfigListenContext> ListenContexts => _configListenContexts;

    #endregion Public 属性

    /// <inheritdoc cref="ConfigBatchListenRequest"/>
    public ConfigBatchListenRequest(bool isListen = true)
    {
        IsListen = isListen;
    }

    #region Public 方法

    public ConfigBatchListenRequest AddListenContext(string @namespace, string dataId, string md5, string group = Constants.DEFAULT_GROUP)
    {
        var ctx = new ConfigListenContext(@namespace, dataId, md5, group);
        _configListenContexts.Add(ctx);
        return this;
    }

    #endregion Public 方法

    #region Public 类

    /// <summary>
    /// 监听内容
    /// </summary>
    public class ConfigListenContext
    {
        #region Public 属性

        [JsonPropertyName("dataId")]
        public string DataId { get; private set; }

        [JsonPropertyName("group")]
        public string Group { get; private set; }

        [JsonPropertyName("md5")]
        public string Md5 { get; private set; }

        [JsonPropertyName("tenant")]
        public string Namespace { get; private set; }

        #endregion Public 属性

        #region Public 构造函数

        /// <inheritdoc cref="ConfigListenContext"/>
        public ConfigListenContext(string @namespace, string dataId, string md5, string group = Constants.DEFAULT_GROUP)
        {
            Namespace = @namespace;
            Group = group;
            DataId = dataId;
            Md5 = md5;
        }

        #endregion Public 构造函数
    }

    #endregion Public 类
}
