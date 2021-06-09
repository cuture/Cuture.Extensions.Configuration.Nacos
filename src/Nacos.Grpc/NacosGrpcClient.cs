using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Nacos.Grpc.GrpcService;
using Nacos.Grpc.Messages;
using Nacos.Internal;
using DuplexStreamingCall = global::Grpc.Core.AsyncDuplexStreamingCall<Nacos.Grpc.GrpcService.Payload, Nacos.Grpc.GrpcService.Payload>;
using GrpcChannel = global::Grpc.Core.Channel;

namespace Nacos.Grpc
{
    /// <inheritdoc cref="INacosClient"/>
    public abstract class NacosGrpcClient : INacosClient
    {
        #region Private 字段

        private readonly IAccessTokenService _accessTokenService;
        private readonly ClientAbilities _clientAbilities;
        private readonly TimeSpan _healthCheckInterval;
        private readonly IHostAddressAccessor _hostAddressAccessor;
        private readonly IMessageSerializer _messageSerializer;
        private readonly string _name;
        private readonly IRequestProcessor _requestProcessor;
        private readonly CancellationTokenSource _runningTokenSource;
        private readonly IServerAddressAccessor _serverAddressAccessor;
        private CancellationTokenSource? _connectionTokenSource;
        private bool _disposedValue;
        private DuplexStreamingCall? _duplexStreamingCall;
        private GrpcChannel? _grpcChannel;
        private bool _isInitiated;
        private NacosTransport.NacosTransportClient? _transportClient;

        #endregion Private 字段

        #region Protected 属性

        /// <summary>
        /// Logger
        /// </summary>
        protected ILogger? Logger { get; }

        /// <summary>
        /// 客户端是否在运行的Token
        /// </summary>
        protected CancellationToken RunningToken { get; }

        #endregion Protected 属性

        #region Public 构造函数

        /// <inheritdoc cref="NacosGrpcClient"/>
        public NacosGrpcClient(NacosGrpcClientOptions clientOptions)
        {
            if (clientOptions is null)
            {
                throw new ArgumentNullException(nameof(clientOptions));
            }
            _healthCheckInterval = clientOptions.HealthCheckInterval;

            if (_healthCheckInterval.TotalSeconds < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(clientOptions), "HealthCheckInterval invalid.");
            }

            _hostAddressAccessor = clientOptions.HostAddressAccessor ?? throw new ArgumentOutOfRangeException(nameof(clientOptions), "HostAddressAccessor invalid.");

            Logger = clientOptions.LoggerFactory?.CreateLogger(GetType());

            _name = clientOptions.ClientName;

            var requestProcessorBuilder = new RequestProcessorBuilder();
            SetupRequestProcessorBuilder(requestProcessorBuilder);
            _requestProcessor = requestProcessorBuilder.Build();

            _serverAddressAccessor = clientOptions.ServerAddressAccessor;

            _accessTokenService = clientOptions.User is null
                                        ? new NoneAccessTokenService()
                                        : new AccessTokenService(clientOptions.User, _serverAddressAccessor, clientOptions.HttpClientFactory, clientOptions.LoggerFactory);

            _messageSerializer = clientOptions.MessageSerializer ?? MessageSerializer.Instance;
            _clientAbilities = clientOptions.ClientAbilities ?? new ClientAbilities();

            _runningTokenSource = new CancellationTokenSource();
            RunningToken = _runningTokenSource.Token;
        }

        #endregion Public 构造函数

        #region Public 方法

        /// <inheritdoc/>
        public virtual async Task InitAsync()
        {
            CheckDisposed();

            if (_isInitiated)
            {
                return;
            }

            try
            {
                await _serverAddressAccessor.InitAsync().ConfigureAwait(false);

                await _accessTokenService.InitAsync().ConfigureAwait(false);

                await TryConnectServerAsync().ConfigureAwait(false);

                _isInitiated = true;
            }
            catch (Exception ex)
            {
                await DisposeAsync().ConfigureAwait(false);
                throw new NacosException("初始化客户端异常", ex);
            }
        }

        #endregion Public 方法

        #region MessageSend

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected async Task<TResponse> RequestAsync<TResponse>(NacosRequest request, CancellationToken token) where TResponse : NacosResponse
        {
            var response = await RequestAsync(request, token).ConfigureAwait(false);

            if (response is not TResponse result)
            {
                throw new NacosException($"无法将响应 - {response} 转换为类型 {typeof(TResponse)}");
            }
            return result;
        }

        /// <inheritdoc cref="RequestAsync{TResponse}(NacosRequest, CancellationToken)"/>
        protected async Task<NacosResponse?> RequestAsync(NacosRequest request, CancellationToken token)
        {
            if (_transportClient is not NacosTransport.NacosTransportClient transportClient)
            {
                throw new NacosException("连接未准备就绪");
            }
            return await InternalRequestAsync(request, transportClient, token).ConfigureAwait(false);
        }

        /// <summary>
        /// 通过双向流发送请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected async Task RequestByBiStreamAsync(NacosRequest request)
        {
            SetRequestAccessToken(request);

            await RequestByBiStreamAsync(_messageSerializer.Serialize(request)).ConfigureAwait(false);
        }

        /// <summary>
        /// 通过双向流发送请求
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        protected async Task RequestByBiStreamAsync(Payload payload)
        {
            if (_duplexStreamingCall is not DuplexStreamingCall duplexStreamingCall)
            {
                throw new NacosException("连接未准备就绪");
            }

            await InternalRequestByBiStreamAsync(payload, duplexStreamingCall).ConfigureAwait(false);
        }

        /// <summary>
        /// 通过双向流发送响应
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        protected async Task ResponseByBiStreamAsync(NacosResponse response)
        {
            await RequestByBiStreamAsync(_messageSerializer.Serialize(response)).ConfigureAwait(false);
        }

        private async Task<NacosResponse> InternalRequestAsync(NacosRequest request, NacosTransport.NacosTransportClient transportClient, CancellationToken token)
        {
            SetRequestAccessToken(request);

            var requestPayload = _messageSerializer.Serialize(request);

            requestPayload.Metadata.ClientIp = _hostAddressAccessor.Address.ToString();

            Logger?.LogDebugSendPayload(requestPayload);

            var responsePayload = await transportClient.RequestAsync(requestPayload, cancellationToken: token);

            Logger?.LogDebugReceivePayload(responsePayload);

            var response = _messageSerializer.Deserialize(responsePayload) as NacosResponse;

            if (response is not null)
            {
                switch (response.ErrorCode)
                {
                    case NacosErrorCode.None:
                    case NacosErrorCode.ConfigurationNotFound:
                        break;

                    case NacosErrorCode.Forbidden:
                        throw new ForbiddenException($"访问被禁止 - Request: {request} Response: {response}");
                    case NacosErrorCode.ConnectionUnRegistered:
                        throw new ConnectionUnRegisteredException($"请求使用的连接未注册 - Request: {request} Response: {response}");
                    default:
                        throw new NacosException($"未知的错误码 - Request: {request} Response: {response}");
                }
                return response;
            }
            throw new NacosException($"请求未正确返回 - Request: {request}");
        }

        private async Task InternalRequestByBiStreamAsync(Payload payload, DuplexStreamingCall duplexStreamingCall)
        {
            payload.Metadata.ClientIp = _hostAddressAccessor.Address.ToString();

            Logger?.LogDebugSendPayload(payload);

            await duplexStreamingCall.RequestStream.WriteAsync(payload).ConfigureAwait(false);
        }

        #endregion MessageSend

        #region BackgroundWork

        private void StartHealthCheck(NacosTransport.NacosTransportClient transportClient, CancellationToken token)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    var healthCheckRequest = new HealthCheckRequest();
                    while (!token.IsCancellationRequested)
                    {
                        await Task.Delay(_healthCheckInterval, token).ConfigureAwait(false);
                        try
                        {
                            var response = await InternalRequestAsync(healthCheckRequest, transportClient, token).ConfigureAwait(false);
                            if (response is null
                                || !response.IsSuccess)
                            {
                                break;
                            }
                            continue;
                        }
                        catch (Exception ex)
                        {
                            token.ThrowIfCancellationRequested();
                            Logger?.LogError(ex, "健康检查出现异常");
                            break;
                        }
                    }
                }
                finally
                {
                    StartReConnectServerLoop();
                }
            }, token);
        }

        private void StartListenServerMessage(DuplexStreamingCall duplexStreamingCall, CancellationToken token)
        {
            _ = Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await ListenServerMessage().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        token.ThrowIfCancellationRequested();
                        Logger?.LogError(ex, "监听服务端消息异常");

                        await Task.Delay(5000, token).ConfigureAwait(false);
                    }
                }
            }, token);

            async Task ListenServerMessage()
            {
                while (!token.IsCancellationRequested
                       && await duplexStreamingCall.ResponseStream.MoveNext(token).ConfigureAwait(false))
                {
                    var messagePayload = duplexStreamingCall.ResponseStream.Current;

                    Logger?.LogDebugReceivePayload(messagePayload);

                    var message = _messageSerializer.Deserialize(messagePayload);

                    if (message is NacosRequest request)
                    {
                        NacosResponse? response = null;

                        try
                        {
                            //HACK pass token?
                            response = await _requestProcessor.HandleAsync(request).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            token.ThrowIfCancellationRequested();
                            Logger?.LogError(ex, "服务端请求处理失败，请求: {0} Origin: {1}", message, messagePayload);
                        }

                        if (response is not null)
                        {
                            response.RequestId = request.RequestId;
                            await ResponseByBiStreamAsync(response).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        Logger?.LogError("不正确的服务端请求：{0} Origin: {1}", message, messagePayload);
                    }
                }
            }
        }

        #endregion BackgroundWork

        #region Private 方法

        private Task<(GrpcChannel Channel, NacosTransport.NacosTransportClient TransportClient, DuplexStreamingCall DuplexStreamingCall)> ConnectServerAsync(ServerUri serverUri)
        {
            Logger?.LogInformation("开始连接服务器 {0} ", serverUri.GrpcUri);

            GrpcChannel grpcChannel = new(serverUri.Host,
                                          serverUri.GrpcPort,
                                          global::Grpc.Core.ChannelCredentials.Insecure,
                                          new List<global::Grpc.Core.ChannelOption> { new global::Grpc.Core.ChannelOption(_name, 1) });

            var transportClient = new NacosTransport.NacosTransportClient(grpcChannel);

            var duplexStreamingCall = transportClient.BiStreamRequest();

            return Task.FromResult((grpcChannel, transportClient, duplexStreamingCall));
        }

        private void SetRequestAccessToken(NacosRequest request)
        {
            if (_accessTokenService.AccessToken is string accessToken
                && !string.IsNullOrWhiteSpace(accessToken))
            {
                request.Headers.AccessToken = accessToken;
            }
        }

        /// <summary>
        /// 设置连接
        /// </summary>
        /// <returns></returns>
        private async Task SetupConnectionAsync(DuplexStreamingCall duplexStreamingCall)
        {
            var connectionSetupRequest = new ConnectionSetupRequest()
            {
                ClientIp = _hostAddressAccessor.Address.ToString(),
                Abilities = _clientAbilities,
            };

            await InternalRequestByBiStreamAsync(_messageSerializer.Serialize(connectionSetupRequest), duplexStreamingCall).ConfigureAwait(false);
        }

        private void StartReConnectServerLoop()
        {
            if (RunningToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                _connectionTokenSource?.Cancel(true);
                _connectionTokenSource?.Dispose();
            }
            catch { }

            _ = Task.Run(async () =>
            {
                Logger?.LogInformation("与Nacos连接断开，开始自动恢复");

                await Task.Delay(TimeSpan.FromSeconds(5), RunningToken).ConfigureAwait(false);

                var scaler = new Scaler(0, 10, 60);

                while (!RunningToken.IsCancellationRequested)
                {
                    await TryDestroyChannel().ConfigureAwait(false);

                    try
                    {
                        var connectionToken = await TryConnectServerAsync().ConfigureAwait(false);
                        OnConnectionRestore(connectionToken);
                        break;
                    }
                    catch (Exception ex)
                    {
                        scaler.Add();

                        Logger?.LogCritical(ex, "自动重连Nacos服务失败, 等待 {0} s 后重试", scaler.Value);
                        await Task.Delay(TimeSpan.FromSeconds(scaler.Value), RunningToken).ConfigureAwait(false);
                    }
                }
            }, RunningToken);
        }

        /// <summary>
        /// 尝试连接Nacos服务
        /// </summary>
        /// <returns>此次连接的有效token</returns>
        private async Task<CancellationToken> TryConnectServerAsync()
        {
            var connectionTokenSource = CancellationTokenSource.CreateLinkedTokenSource(RunningToken);

            var token = connectionTokenSource.Token;

            GrpcChannel? channel = null;
            DuplexStreamingCall? duplexStreamingCall = null;

            for (int i = 0; i < _serverAddressAccessor.Count || i < 3; i++)
            {
                token.ThrowIfCancellationRequested();

                var server = _serverAddressAccessor.CurrentAddress;
                try
                {
                    NacosTransport.NacosTransportClient? transportClient;
                    (channel, transportClient, duplexStreamingCall) = await ConnectServerAsync(server).ConfigureAwait(false);

                    await SetupConnectionAsync(duplexStreamingCall).ConfigureAwait(false);

                    //确认连接可用
                    for (int tryCount = 0; tryCount <= 5; tryCount++)
                    {
                        //如果不等待，马上发消息会提示连接未注册
                        await Task.Delay(1000, token).ConfigureAwait(false);

                        try
                        {
                            await InternalRequestAsync(new HealthCheckRequest(), transportClient, token).ConfigureAwait(false);
                        }
                        catch (ConnectionUnRegisteredException)
                        {
                            if (tryCount == 5)
                            {
                                throw;
                            }
                        }
                    }

                    StartListenServerMessage(duplexStreamingCall, token);

                    StartHealthCheck(transportClient, token);

                    Logger?.LogInformation("连接服务器 {0} 完成", server.GrpcUri);

                    _connectionTokenSource = connectionTokenSource;

                    _grpcChannel = channel;
                    _transportClient = transportClient;
                    _duplexStreamingCall = duplexStreamingCall;

                    return connectionTokenSource.Token;
                }
                catch (Exception ex)
                {
                    await DisposeTempResourcesAsync().ConfigureAwait(false);
                    Logger?.LogError(ex, "尝试连接服务器 {0} 失败", server.GrpcUri);
                    _serverAddressAccessor.MoveNextAddress();
                }
            }

            DisposeConnectionTokenSource();

            throw new NacosException("尝试连接所有服务器失败");

            async Task DisposeTempResourcesAsync()
            {
                if (channel is not null)
                {
                    try
                    {
                        duplexStreamingCall?.Dispose();

                        await channel.ShutdownAsync().ConfigureAwait(false);
                    }
                    catch { }

                    channel = null;
                }
            }

            void DisposeConnectionTokenSource()
            {
                connectionTokenSource.Cancel(true);
                connectionTokenSource.Dispose();
            }
        }

        private async Task TryDestroyChannel()
        {
            if (_grpcChannel is GrpcChannel channel)
            {
                _grpcChannel = null;
                _transportClient = null;
                try
                {
                    if (_duplexStreamingCall is DuplexStreamingCall duplexStreamingCall)
                    {
                        duplexStreamingCall.Dispose();
                    }
                    _duplexStreamingCall = null;

                    await channel.ShutdownAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Logger?.LogError(ex, "关闭旧的连接失败");
                }
            }
        }

        #endregion Private 方法

        #region Protected 方法

        /// <summary>
        /// 检查是否已释放
        /// </summary>
        protected virtual void CheckDisposed()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(_grpcChannel));
            }
        }

        /// <summary>
        /// 检查是否未初始化
        /// </summary>
        protected virtual void CheckInitiated()
        {
            if (!_isInitiated)
            {
                throw new NacosException("使用Client前应该先初始化");
            }
        }

        /// <summary>
        /// 连接恢复
        /// </summary>
        /// <param name="token"></param>
        protected abstract void OnConnectionRestore(CancellationToken token);

        /// <summary>
        /// 设置请求处理器
        /// </summary>
        /// <param name="builder"></param>
        protected abstract void SetupRequestProcessorBuilder(RequestProcessorBuilder builder);

        #endregion Protected 方法

        #region Dispose

        /// <summary>
        ///
        /// </summary>
        ~NacosGrpcClient()
        {
            InternalDispose(disposing: false, true);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            InternalDispose(disposing: true, true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            if (!_disposedValue)
            {
                InternalDispose(disposing: true, false);

                if (_grpcChannel is GrpcChannel grpcChannel)
                {
                    await grpcChannel.ShutdownAsync().ConfigureAwait(false);
                }
            }
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

                _connectionTokenSource?.Cancel(true);
                _connectionTokenSource?.Dispose();

                _runningTokenSource.Cancel(true);
                _runningTokenSource.Dispose();
            }
        }

        private void InternalDispose(bool disposing, bool shutdownChannel)
        {
            Dispose(disposing);
            if (shutdownChannel
                && _grpcChannel is GrpcChannel grpcChannel)
            {
                _ = grpcChannel.ShutdownAsync();
            }
        }

        #endregion Dispose
    }
}