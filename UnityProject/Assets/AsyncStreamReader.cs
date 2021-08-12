using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

internal class AsyncStreamReader<T> : AsyncStreamReader, IAsyncStreamReader<T>
{
    public new T Current => (T) base.Current;

    public void AddItem(T item) => AddItem((object) item);

    public override void AddItem(object item)
    {
        if (item is T) base.AddItem(item);
        else throw new InvalidCastException(
            $"Bad item for stream. Expected {typeof(T).Name} got {item?.GetType().Name ?? "null"}");
    }
}

internal class AsyncStreamReader : IAsyncStreamReader<object>
{
    public object Current { get; private set; }
    private bool _isFinished;
    private readonly Queue<object> _queue = new Queue<object>();
    private TaskCompletionSource<object> _event = new TaskCompletionSource<object>();

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
            await _event.Task.WaitAsync(cancellationToken);
        }
    }

    public void SignalEnd()
    {
        _isFinished = true;
        _event.SetResult(null);
        _event = new TaskCompletionSource<object>();
    }

    public void SignalError(Exception e)
    {
        _event.SetException(e);
        _event = new TaskCompletionSource<object>();
    }

    public virtual void AddItem(object item)
    {
        _queue.Enqueue(item);
        _event.SetResult(null);
        _event = new TaskCompletionSource<object>();
    }
}
