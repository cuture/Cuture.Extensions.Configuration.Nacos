using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Nacos.Http.Messages;

namespace Nacos
{
    /// <inheritdoc cref="IAccessTokenService"/>
    public class AccessTokenService : IAccessTokenService
    {
        #region Private 字段

        private readonly INacosUnderlyingHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccessTokenService>? _logger;
        private readonly CancellationToken _runningToken;

        private readonly CancellationTokenSource _runningTokenSource;
        private readonly IServerAddressAccessor _serverAddressAccessor;
        private readonly NacosUser _user;
        private AccessTokenWrapper _accessTokenWrapper = new(string.Empty, -1);

        private CancellationTokenSource? _autoRefreshTokenSource;

        private bool _disposedValue;
        private bool _isInitiated;

        #endregion Private 字段

        #region Private 属性

        private CancellationTokenSource? AutoRefreshTokenSource
        {
            set
            {
                var existValue = Interlocked.Exchange(ref _autoRefreshTokenSource, value);
                if (existValue is not null)
                {
                    existValue.Cancel(true);
                    existValue.Dispose();
                }
            }
        }

        #region Public 属性

        /// <inheritdoc/>
        public string AccessToken => _accessTokenWrapper.Token ?? throw new NacosException("没有可用的 AccessToken");

        #endregion Public 属性

        #endregion Private 属性

        #region Public 构造函数

        /// <inheritdoc cref="AccessTokenService"/>
        public AccessTokenService(NacosUser user, IServerAddressAccessor serverAddressAccessor, INacosUnderlyingHttpClientFactory httpClientFactory, ILoggerFactory? loggerFactory = null)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));

            _serverAddressAccessor = serverAddressAccessor ?? throw new ArgumentNullException(nameof(serverAddressAccessor));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = loggerFactory?.CreateLogger<AccessTokenService>();

            _runningTokenSource = new CancellationTokenSource();
            _runningToken = _runningTokenSource.Token;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public async Task InitAsync()
        {
            if (_isInitiated)
            {
                return;
            }

            var loginResponse = await LoginAsync().ConfigureAwait(false);

            var ttl = loginResponse.TokenTtl;

            _accessTokenWrapper = new(loginResponse.AccessToken, ttl);

            StartAutoTokenRefresh(ttl);

            _isInitiated = true;
        }

        /// <inheritdoc/>
        public async Task<string> RefreshAccessTokenAsync()
        {
            var loginResponse = await LoginAsync().ConfigureAwait(false);
            _accessTokenWrapper = new(loginResponse.AccessToken, loginResponse.TokenTtl);
            return loginResponse.AccessToken;
        }

        #endregion Public 方法

        #region Private 方法

        private async Task<LoginResponse> LoginAsync(CancellationToken token = default)
        {
            var loginRequest = new LoginRequest(_user.Account, _user.Password);

            for (int i = 0; i < _serverAddressAccessor.Count || i < 3; i++)
            {
                var server = _serverAddressAccessor.CurrentAddress;

                HttpResponseMessage? responseMessage = null;
                string? content = null;

                try
                {
                    using var httpRequest = loginRequest.ToHttpRequestMessage(server);

                    using var client = _httpClientFactory.CreateClient(server.HttpUri);

                    _logger?.LogInformation("使用用户名 {0} 登录 {1}", loginRequest.Account, server);

                    responseMessage = await client.SendAsync(httpRequest, token).ConfigureAwait(false);

                    content = await responseMessage.Content.ReadAsStringAsync(token).ConfigureAwait(false);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var response = JsonSerializer.Deserialize<LoginResponse>(content);

                        if (response is not null)
                        {
                            _logger?.LogInformation("使用用户名 {0} 登录 {1} 成功", _user.Account, server);
                            return response;
                        }
                        throw new LoginFailException($"响应内容 {content} 无法正确序列化");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "请求执行失败 {0} - TargetServer: {1}", loginRequest, server);
                    _serverAddressAccessor.MoveNextAddress();
                    continue;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new LoginFailException($"Nacos登录失败，拒绝访问 - StatusCode: {(int)responseMessage.StatusCode} Message: {content}");
                }
                else
                {
                    _logger?.LogError("使用用户名 {0} 登录 {1} 失败 - StatusCode: {2} Message: {3}", loginRequest.Account, server, responseMessage.StatusCode, content);
                }
                _serverAddressAccessor.MoveNextAddress();
            }

            throw new LoginFailException("Nacos登录失败 - 已尝试所有服务地址");
        }

        private void StartAutoTokenRefresh(int tokenTtl)
        {
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(_runningToken);
            AutoRefreshTokenSource = tokenSource;

            var cancellationToken = tokenSource.Token;

            Task.Factory.StartNew(async () =>
            {
                //失败计数
                int failCount = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    //有效期还有三分之一时，进行刷新
                    var interval = TimeSpan.FromSeconds(tokenTtl * 0.66);

                    _logger?.LogInformation("{0} s 后自动刷新AccessToken", interval.TotalSeconds);
                    await Task.Delay(interval, cancellationToken).ConfigureAwait(false);

                    //循环尝试
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            _logger?.LogInformation("开始自动刷新AccessToken");

                            var loginResponse = await LoginAsync(cancellationToken).ConfigureAwait(false);

                            tokenTtl = loginResponse.TokenTtl;

                            Interlocked.Exchange(ref _accessTokenWrapper, new(loginResponse.AccessToken, tokenTtl));

                            _logger?.LogInformation("自动刷新AccessToken成功，Token: {0} Ttl: {1}", loginResponse.AccessToken, tokenTtl);

                            failCount = 0;

                            break;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            var retryDelaySeconds = failCount > 12 ? 120 : failCount * 10;
                            _logger?.LogError(ex, "自动刷新AccessToken出现异常，等待 {0} s 后再次进行尝试", retryDelaySeconds);

                            await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }, cancellationToken);
        }

        #endregion Private 方法

        #region Dispose

        /// <summary>
        ///
        /// </summary>
        ~AccessTokenService()
        {
            Dispose(disposing: false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;

                AutoRefreshTokenSource = null!;

                _runningTokenSource.Cancel(true);
                _runningTokenSource.Dispose();
            }
        }

        #endregion Dispose

        #region Private 类

        private class AccessTokenWrapper
        {
            #region Private 字段

            private readonly string _token;

            #endregion Private 字段

            #region Public 属性

            public DateTimeOffset Expire { get; }

            public string? Token => Expire > DateTimeOffset.UtcNow ? _token : null;

            #endregion Public 属性

            #region Public 构造函数

            public AccessTokenWrapper(string token, int ttl)
            {
                _token = token;

                //过期时间还有十分之一时提前过期
                Expire = DateTimeOffset.UtcNow.AddSeconds(ttl - (ttl / 10));
            }

            #endregion Public 构造函数
        }

        #endregion Private 类
    }
}