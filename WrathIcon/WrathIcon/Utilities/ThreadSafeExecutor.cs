using System;
using System.Threading.Tasks;

namespace WrathIcon.Utilities
{
    public static class ThreadSafeExecutor
    {
        public static void RunOnMainThread(Action action)
        {
            if (Plugin.Framework.IsInFrameworkUpdateThread)
            {
                action();
            }
            else
            {
                Plugin.Framework.RunOnTick(action);
            }
        }
        
        public static Task<T> RunOnMainThreadAsync<T>(Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            RunOnMainThread(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
        
        public static Task RunOnMainThreadAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            RunOnMainThread(() =>
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
} 