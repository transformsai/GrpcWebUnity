using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcWebUnity.Internal;

namespace GrpcWebUnity
{
    public static class GrpcWeb
    {
        public static async Task<ChannelBase> GetChannelAsync(string targetAddress,
            ChannelCredentials credentials = null, CancellationToken cancellationToken = default)
        {
            credentials ??= ChannelCredentials.Insecure;

#if UNITY_WEBGL && !UNITY_EDITOR
            await GrpcWebUnity.Internal.GrpcWebConnector.Instance.WaitForInitialization.WaitAsync(cancellationToken);
            return Instance.MakeChannel(targetAddress);
#else
            return new Channel(targetAddress, credentials);
#endif
        }

    }


    public static class Utils
    {
        public static Task<TNewResult> ContinueWithSync<TResult, TNewResult>(this Task<TResult> task,
            Func<Task<TResult>, TNewResult> contFunc, CancellationToken token = default) =>
            task.ContinueWith(contFunc, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

        public static Task ContinueWithSync<TResult>(this Task<TResult> task,
            Action<Task<TResult>> contFunc, CancellationToken token = default) =>
            task.ContinueWith(contFunc, token, TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);
    }
}
