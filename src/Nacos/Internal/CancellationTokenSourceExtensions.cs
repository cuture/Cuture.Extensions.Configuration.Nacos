namespace System.Threading;

internal static class CancellationTokenSourceExtensions
{
    #region Public 方法

    /// <summary>
    /// 静默释放（Cancel并Dispose，吞噬所有异常）
    /// </summary>
    /// <param name="cancellationTokenSource"></param>
    public static void SilenceRelease(this CancellationTokenSource? cancellationTokenSource)
    {
        if (cancellationTokenSource is null)
        {
            return;
        }
        try
        {
            cancellationTokenSource.Cancel();
        }
        catch { }
        finally
        {
            cancellationTokenSource.Dispose();
        }
    }

    #endregion Public 方法
}
