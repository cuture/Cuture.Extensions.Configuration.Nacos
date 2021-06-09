namespace System.Threading.Tasks
{
    internal static class TaskExtensions
    {
        #region Public 方法

        public static void WaitWithoutContext(this Task task)
        {
            if (task.IsCompleted)
            {
                return;
            }

            task.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static TResult WaitWithoutContext<TResult>(this Task<TResult> task)
        {
            if (task.IsCompleted)
            {
                return task.Result;
            }

            task.ConfigureAwait(false).GetAwaiter().GetResult();
            return task.Result;
        }

        #endregion Public 方法
    }
}