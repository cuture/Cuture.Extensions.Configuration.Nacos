namespace Nacos.Http
{
    /// <summary>
    /// NacosHttp客户端选项
    /// </summary>
    public class NacosHttpClientOptions : NacosClientOptions
    {
        #region Public 构造函数

        /// <inheritdoc cref="NacosHttpClientOptions"/>
        public NacosHttpClientOptions(string clientName,
                                      IServerAddressAccessor serverAddressAccessor,
                                      INacosUnderlyingHttpClientFactory? httpClientFactory = null)
            : base(clientName, serverAddressAccessor, httpClientFactory)
        {
        }

        #endregion Public 构造函数
    }
}