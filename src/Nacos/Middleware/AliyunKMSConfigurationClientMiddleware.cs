using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using Microsoft.Extensions.Logging;

using Nacos.Exceptions;
using Nacos.Utils;

namespace Nacos.Middleware
{
    /// <summary>
    /// 阿里云ACM KMS解密Nacos配置中间件
    /// </summary>
    public class AliyunKMSConfigurationClientMiddleware : INacosConfigurationClientMiddleware
    {
        #region Private 字段

        private readonly AliyunAcsProfile _acsProfile;
        private readonly INacosUnderlyingHttpClientFactory _httpClientFactory;
        private readonly ILogger? _logger;

        #endregion Private 字段

        #region Public 构造函数

        /// <inheritdoc cref="AliyunKMSConfigurationClientMiddleware"/>
        public AliyunKMSConfigurationClientMiddleware(AliyunAcsProfile acsProfile, ILogger<AliyunKMSConfigurationClientMiddleware>? logger = null, INacosUnderlyingHttpClientFactory? httpClientFactory = null)
        {
            if (acsProfile is null)
            {
                throw new ArgumentNullException(nameof(acsProfile));
            }

            _acsProfile = acsProfile;
            _logger = logger;
            _httpClientFactory = httpClientFactory ?? DefaultNacosUnderlyingHttpClientFactory.Shared;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public async Task<NacosConfigurationDescriptor> InvokeGetConfigurationAsync(ConfigurationGetContext context, Func<ConfigurationGetContext, Task<NacosConfigurationDescriptor>> next)
        {
            var result = await next(context).ConfigureAwait(false);

            if (context.Descriptor.DataId.StartsWith("cipher-", StringComparison.OrdinalIgnoreCase))
            {
                _logger?.LogDebug("start decrypt kms encrypt configuration {0}", context.Descriptor);

                var decryptedContent = await DecryptConfigurationAsync(result, context.CancellationToken).ConfigureAwait(false);

                return result.WithContent(decryptedContent);
            }
            return result;
        }

        #endregion Public 方法

        #region Private 方法

        private async Task<string> DecryptConfigurationAsync(NacosConfigurationDescriptor descriptor, CancellationToken cancellationToken)
        {
            var ciphertextBlob = descriptor.Content;

            //HACK 暂时不需要其它的阿里云请求，直接硬编码请求加密流程

            var queryString = $"AccessKeyId={_acsProfile.AccessKeyId}&Action=Decrypt&CiphertextBlob={ciphertextBlob}&Format=JSON&RegionId={_acsProfile.RegionId}&SignatureMethod=HMAC-SHA1&SignatureNonce={Guid.NewGuid()}&SignatureVersion=1.0&Timestamp={Uri.EscapeDataString(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"))}&Version=2016-01-20";

            //HACK acs地址获取，是否还需要其它特别处理？
            var host = $"https://kms.{_acsProfile.RegionId}.aliyuncs.com";

            var signatureData = HashUtil.ComputeHMACSHA1($"POST&%2F&{Uri.EscapeDataString(queryString)}", $"{_acsProfile.AccessKeySecret}&");

            var signature = HttpUtility.UrlEncode(Convert.ToBase64String(signatureData));

            _logger?.LogDebug("Configuration {0} Kms Decrypt Signature {1}", descriptor, signature);

            var url = $"{host}?{queryString}&Signature={signature}";

            var uri = new Uri(url);

            _logger?.LogDebug("Configuration {0} Kms Decrypt request url {1}", descriptor, url);

            using var httpclient = _httpClientFactory.CreateClient(uri);

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);

            var responseMessage = await httpclient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

            var response = await responseMessage.Content.ReadFromJsonAsync<KmsDecryptResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

            _logger?.LogDebug("Configuration {0} Kms Decrypt Result {1}", descriptor, response?.Plaintext);

            if (response?.Plaintext is null)
            {
                throw new NacosException($"Kms Decrypt configuration - {descriptor} fail.");
            }
            return response.Plaintext;
        }

        #endregion Private 方法

        #region Private 类

        private class KmsDecryptResponse
        {
            #region Public 属性

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            public string KeyId { get; set; }
            public string KeyVersionId { get; set; }
            public string Plaintext { get; set; }
            public string RequestId { get; set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

            #endregion Public 属性
        }

        #endregion Private 类
    }
}