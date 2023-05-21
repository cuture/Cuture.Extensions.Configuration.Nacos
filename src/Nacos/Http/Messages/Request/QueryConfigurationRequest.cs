using System.Net.Http;

namespace Nacos.Http.Messages;

/// <summary>
/// 查询配置请求
/// </summary>
public class QueryConfigurationRequest : NacosHttpRequest, INacosUniqueConfiguration
{
    #region Public 属性

    /// <inheritdoc/>
    public string DataId { get; private set; }

    /// <inheritdoc/>
    public string Group { get; private set; }

    /// <inheritdoc/>
    public string Namespace { get; private set; }

    #endregion Public 属性

    #region Public 构造函数

    /// <inheritdoc cref="QueryConfigurationRequest"/>
    public QueryConfigurationRequest(NacosConfigurationDescriptor descriptor)
    {
        Namespace = descriptor.Namespace;
        DataId = descriptor.DataId;
        Group = descriptor.Group;
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <inheritdoc/>
    public override string? GetSpasSignData() => $"{Namespace}+{Group}";

    /// <inheritdoc/>
    public string GetUniqueKey() => INacosUniqueConfiguration.GenerateUniqueKey(this);

    /// <inheritdoc/>
    public override HttpRequestMessage ToHttpRequestMessage(ServerUri uri)
    {
        //HACK UrlEncode？
        var query = $"tenant={Namespace}&dataId={DataId}&group={Group}";
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, MakeUri(uri, "nacos/v1/cs/configs", query));

        return LoadRequestHeaders(httpRequestMessage);
    }

    #endregion Public 方法
}
