using System;
using System.Text.RegularExpressions;

namespace Nacos
{
    /// <summary>
    /// Nacos服务地址
    /// </summary>
    public sealed class ServerUri
    {
        #region Private 字段

        private readonly bool _isAliyunAcm;
        private Uri? _grpcUri;
        private Uri? _httpUri;

        #endregion Private 字段

        #region Public 属性

        /// <summary>
        /// Grpc 端口
        /// </summary>
        public int GrpcPort { get; private set; }

        /// <summary>
        /// GrpcUri
        /// </summary>
        public Uri GrpcUri => _grpcUri ??= CreateUri(GrpcPort);

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; private set; }

        /// <summary>
        /// Http 端口
        /// </summary>
        public int HttpPort { get; private set; }

        /// <summary>
        /// HttpUri
        /// </summary>
        public Uri HttpUri => _httpUri ??= CreateUri(HttpPort);

        /// <summary>
        /// 是否是Https
        /// </summary>
        public bool IsSecurityConnection { get; private set; }

        /// <summary>
        /// uri方案名称
        /// </summary>
        public string Scheme { get; private set; }

        #endregion Public 属性

        #region Private 构造函数

        /// <inheritdoc cref="ServerUri"/>
        public ServerUri(Uri acmServerListUri)
        {
            _httpUri = acmServerListUri;
            _isAliyunAcm = true;
            Host = acmServerListUri.Host;
            Scheme = acmServerListUri.Scheme;
        }

        /// <inheritdoc cref="ServerUri"/>
        private ServerUri(string host, int httpPort, int grpcPort, string scheme)
        {
            Host = host;
            HttpPort = httpPort;
            GrpcPort = grpcPort;
            Scheme = scheme;
            IsSecurityConnection = scheme.Contains("https", StringComparison.Ordinal);
        }

        #endregion Private 构造函数

        #region Public 方法

        /// <summary>
        /// 转换为 <see cref="ServerUri"/>
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static ServerUri Parse(string url) => Parse(new Uri(url));

        /// <summary>
        /// 转换为 <see cref="ServerUri"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static ServerUri Parse(Uri uri)
        {
            if (uri is null)
            {
                throw new ArgumentNullException(nameof(uri));
            }
            var httpPort = Constants.DEFAULT_HTTP_PORT;
            var grpcPort = Constants.DEFAULT_GRPC_PORT;
            var host = uri.Host;

            var uriScheme = uri.Scheme.ToLowerInvariant();

            if (uriScheme.Contains("acm", StringComparison.Ordinal))
            {
                var uriBuilder = new UriBuilder(uri)
                {
                    Scheme = GetScheme(uriScheme)
                };

                if (uriBuilder.Port == -1)
                {
                    uriBuilder.Port = 8080;
                }
                if (string.IsNullOrWhiteSpace(uriBuilder.Path)
                    || uriBuilder.Path.Length == 1)
                {
                    uriBuilder.Path = "/nacos/serverlist";
                }
                var acmServerListUri = uriBuilder.Uri;
                return new ServerUri(acmServerListUri);
            }
            else if (uriScheme.Contains("grpc", StringComparison.Ordinal))
            {
                grpcPort = uri.Port;

                if (!TryMatchPort(uri.Fragment, "HttpPort", ref httpPort))
                {
                    httpPort = grpcPort - Constants.DEFAULT_GRPC_PORT_OFFSET;
                }
            }
            else if (uriScheme.Contains("http", StringComparison.Ordinal))
            {
                httpPort = uri.Port;
                if (!TryMatchPort(uri.Fragment, "GrpcPort", ref grpcPort))
                {
                    grpcPort = httpPort + Constants.DEFAULT_GRPC_PORT_OFFSET;
                }
            }
            else
            {
                throw new ArgumentException("无法转换为Nacos服务地址", nameof(uri));
            }

            uriScheme = GetScheme(uriScheme);

            return new ServerUri(host, httpPort, grpcPort, uriScheme);

            static string GetScheme(string uriScheme)
            {
                uriScheme = uriScheme.Contains("https", StringComparison.OrdinalIgnoreCase)
                                ? "https"
                                : "http";
                return uriScheme;
            }
        }

        /// <summary>
        /// 是否为阿里云ACM
        /// </summary>
        /// <returns></returns>
        public bool IsAliyunAcm() => _isAliyunAcm;

        /// <inheritdoc/>
        public override string ToString() => HttpUri.ToString();

        #endregion Public 方法

        #region Private 方法

        private static bool TryMatchPort(string input, string key, ref int grpcPort)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            if (Regex.Match(input, $"{key}=(\\d+)", RegexOptions.IgnoreCase) is Match match
                && match.Success)
            {
                if (!int.TryParse(match.Groups[1].Value, out grpcPort))
                {
                    throw new NacosException($"无法将 {match.Groups[1].Value} 转换为端口号");
                }
                return true;
            }
            return false;
        }

        private Uri CreateUri(int port)
        {
            return new UriBuilder()
            {
                Scheme = Scheme,
                Host = Host,
                Port = port,
            }.Uri;
        }

        #endregion Private 方法
    }
}