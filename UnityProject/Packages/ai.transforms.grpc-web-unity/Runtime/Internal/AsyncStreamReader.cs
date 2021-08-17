using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace GrpcWebUnity.Internal
{
    internal class AsyncStreamReader<T> : AsyncStreamReader, IAsyncStreamReader<T>
    {
        public new T Current => (T) base.Current;

        public AsyncStreamReader(CancellationToken masterToken) : base(masterToken)
        {
        }

        public void AddItem(T item) => AddItem((object) item);

        public override void AddItem(object item)
        {
            if (item is T) base.AddItem(item);
            else
                throw new InvalidCastException(
                    $"Bad item for stream. Expected {typeof(T).Name} got {item?.GetType().Name ?? "null"}");
        }

    }

    internal class AsyncStreamReader : IAsyncStreamReader<object>
    {
        public object Current { get; private set; }

        private readonly CancellationToken _masterToken;
        private bool _isFinished;
        private readonly Queue<object> _queue = new Queue<object>();
        private TaskCompletionSource<object> _event = new TaskCompletionSource<object>();


        public AsyncStreamReader(CancellationToken masterToken) => _masterToken = masterToken;

        public async Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (_queue.Count > 0)
                {
                    Current = _queue.Dequeue();
                    return true;
                }

                if (_isFinished) return false;
                var mergedToken = cancellationToken;

                if (cancellationToken != _masterToken)
                    mergedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _masterToken)
                        .Token;

                await _event.Task.WaitAsync(mergedToken);
            }
        }

        public void SignalEnd()
        {
            _isFinished = true;
            var oldEvent = _event;
            _event = new TaskCompletionSource<object>();
            oldEvent.SetResult(null);
        }

        public void SignalError(Exception e)
        {
            var oldEvent = _event;
            _event = new TaskCompletionSource<object>();
            oldEvent.SetException(e);
        }

        public virtual void AddItem(object item)
        {
            _queue.Enqueue(item);
            var oldEvent = _event;
            _event = new TaskCompletionSource<object>();
            oldEvent.SetResult(null);
        }
    }

}
