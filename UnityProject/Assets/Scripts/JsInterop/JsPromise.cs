using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public class JsPromise : JsObject
{

    internal JsPromise(double refId) : base(refId, JsTypes.Promise) { }
    
    private Task<JsReference> _task;

    public Task<JsReference> Task
    {
        get
        {
            if (_task != null) return _task;

            var tcs = new TaskCompletionSource<JsReference>();

            JsCallback onAccept = null;
            JsCallback onReject = null;

            void CleanUp()
            {
                // ReSharper disable AccessToModifiedClosure
                onAccept?.Dispose();
                onReject?.Dispose();
                // ReSharper enable AccessToModifiedClosure
            }

            onAccept = JsCallback.Create((JsReference reference) =>
            {
                tcs.SetResult(reference);
                CleanUp();
            });
            onReject = JsCallback.Create((JsReference reference) =>
            {
                tcs.TrySetException(new Exception($"Javascript Promise error: \n{reference}"));
                CleanUp();
            });

            Then(onAccept, onReject);
            return _task = tcs.Task;
        }
    }

    public JsPromise Then(JsFunction onAccept, JsFunction onReject = null) => Invoke("then", onAccept, onReject) as JsPromise;

    public TaskAwaiter<JsReference> GetAwaiter() => Task.GetAwaiter();
}
