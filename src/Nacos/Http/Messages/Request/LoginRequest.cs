using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Nacos.Http.Messages
{
    /// <summary>
    /// 登陆请求
    /// </summary>
    public class LoginRequest : NacosHttpRequest
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <inheritdoc cref="LoginRequest"/>
        public LoginRequest(string account, string password)
        {
            if (string.IsNullOrWhiteSpace(account))
            {
                throw new ArgumentException($"“{nameof(account)}”不能为 null 或空白。", nameof(account));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"“{nameof(password)}”不能为 null 或空白。", nameof(password));
            }

            Account = account;
            Password = password;
        }

        /// <inheritdoc/>
        public override HttpRequestMessage ToHttpRequestMessage(ServerUri uri)
        {
            return new HttpRequestMessage(HttpMethod.Post, MakeUri(uri, "nacos/v1/auth/login"))
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string?, string?>("username", Account),
                    new KeyValuePair<string?, string?>("password", Password)
                })
            };
        }
    }
}