using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace GrpcWebUnity.Internal
{
    public static class Utils
    {
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

        public static long ToUnixTimeSeconds(this DateTime? time) =>
            time.HasValue ? ((DateTimeOffset)time.Value).ToUnixTimeSeconds() : 0;

        private const string BinarySuffix = "-bin";
        public static string EncodeMetadata(this Metadata metadata)
        {
            if (metadata.Count == 0) return "";
            var builder = new StringBuilder();
            foreach (var entry in metadata)
            {
                var key = entry.Key;
                if (entry.IsBinary && !key.EndsWith(BinarySuffix)) key += BinarySuffix;
                builder.Append(ToBase64Utf8(key));
                builder.Append('|');
                builder.Append(entry.IsBinary ? Convert.ToBase64String(entry.ValueBytes) : ToBase64Utf8(entry.Value));
                builder.AppendLine();

            }
            return builder.ToString();
        }
        public static Metadata DecodeMetadata(StringReader sr)
        {
            Metadata metadata = null;

            while (true)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                metadata ??= new Metadata();
                var split = line.Split('|');
                var key = FromBase64Utf8(split[0]);
                var isBinary = key.EndsWith("-bin");
                if (isBinary)
                {
                    key = key.Substring(0, key.Length - BinarySuffix.Length);
                    metadata.Add(key, Convert.FromBase64String(split[1]));
                }
                else
                {
                    metadata.Add(key, FromBase64Utf8(split[1]));
                }

            }

            return metadata;
        }

        internal static string ToBase64Utf8(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(bytes);
        }

        internal static string FromBase64Utf8(string str)
        {
            var bytes = Convert.FromBase64String(str);
            return Encoding.UTF8.GetString(bytes);
        }

        public static Task<TNewResult> ContinueWithSync<TResult, TNewResult>(this Task<TResult> task,
            Func<Task<TResult>, TNewResult> contFunc, CancellationToken token = default) =>
            task.ContinueWith(contFunc, token, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);

        public static Task ContinueWithSync<TResult>(this Task<TResult> task,
            Action<Task<TResult>> contFunc, CancellationToken token = default) =>
            task.ContinueWith(contFunc, token, TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);
    }

}
