using System.Runtime.CompilerServices;

using Nacos.Grpc.GrpcService;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        #region Public 方法

        public static void LogDebugReceivePayload(this ILogger? logger, Payload payload, [CallerMemberName] string? method = null)
        {
            if (logger is null
                || !logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            logger?.LogDebug("Method 【{0}】 Receive Payload Type:【{1}】 Metadata: {2} - Body: {3}", method, payload.Metadata.Type, payload.Metadata, payload.Body.Value.ToStringUtf8());
        }

        public static void LogDebugSendPayload(this ILogger? logger, Payload payload, [CallerMemberName] string? method = null)
        {
            if (logger is null
                || !logger.IsEnabled(LogLevel.Debug))
            {
                return;
            }

            logger?.LogDebug("Method 【{0}】 Send Payload Type:【{1}】 Metadata: {2} - Body: {3}", method, payload.Metadata.Type, payload.Metadata, payload.Body.Value.ToStringUtf8());
        }

        #endregion Public 方法
    }
}