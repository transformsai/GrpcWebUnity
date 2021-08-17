using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcWebUnity
{
    public static class Utils
    {
        public static Task<TNewResult> ContinueWithSync<TResult, TNewResult>(this Task<TResult> task,
            Func<Task<TResult>, TNewResult> contFunc, CancellationToken token = default) =>
            task.ContinueWith(contFunc, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

        public static Task ContinueWithSync<TResult>(this Task<TResult> task,
            Action<Task<TResult>> contFunc, CancellationToken token = default) =>
            task.ContinueWith(contFunc, token, TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);


        public static async Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!cancellationToken.CanBeCanceled)
            {
                await task;
                return;
            }

            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<object>();
            using (cancellationToken.Register(tcs.SetCanceled))
            {
                await Task.WhenAny(task, tcs.Task);
            }
        }

        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (!cancellationToken.CanBeCanceled)
                return await task;

            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var tcs = new TaskCompletionSource<T>();
            using (cancellationToken.Register(tcs.SetCanceled))
            {
                var result = await Task.WhenAny(task, tcs.Task);
                return await result;
            }
        }
    }
}
