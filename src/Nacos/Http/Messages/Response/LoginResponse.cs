using System;
using System.Text.Json.Serialization;

namespace Nacos.Http.Messages
{
    /// <summary>
    /// 登录响应
    /// </summary>
    public class LoginResponse : NacosHttpResponse
    {
        #region Public 属性

        /// <summary>
        /// token
        /// </summary>
        [JsonPropertyName(Constants.ACCESS_TOKEN)]
        public string AccessToken { get; set; }

        /// <summary>
        /// 过期时间 （应该是秒）
        /// </summary>
        [JsonPropertyName(Constants.TOKEN_TTL)]
        public int TokenTtl { get; set; }

        #endregion Public 属性

        /// <inheritdoc cref="LoginResponse"/>
        public LoginResponse(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentException($"“{nameof(accessToken)}”不能为 null 或空白。", nameof(accessToken));
            }

            AccessToken = accessToken;
        }
    }
}