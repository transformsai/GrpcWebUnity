using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JsInterop.Internal;

namespace JsInterop.Types
{
    public class JsPromise : JsObject {

        private Task<JsValue> _task;
        public Task<JsValue> Task => _task ?? (_task = CreateTask());

        internal JsPromise(double refId) : base(refId, JsTypes.Promise) { }

        public JsPromise Then(JsFunction onAccept, JsFunction onReject = null) => Invoke("then", onAccept, onReject ?? JsValue.Undefined).As<JsPromise>();
        public TaskAwaiter<JsValue> GetAwaiter() => Task.GetAwaiter();

        private Task<JsValue> CreateTask()
        {
            var tcs = new TaskCompletionSource<JsValue>();

            JsCallback onAccept = null;
            JsCallback onReject = null;

            void CleanUp()
            {
                // ReSharper disable AccessToModifiedClosure
                onAccept?.Dispose();
                onReject?.Dispose();
                // ReSharper enable AccessToModifiedClosure
            }

            void AcceptAction(JsValue reference)
            {
                tcs.SetResult(reference);
                CleanUp();
            }

            void RejectAction(JsValue reference)
            {
                tcs.TrySetException(new Exception($"Javascript Promise error: \n{reference}"));
                CleanUp();
            }

            onAccept = Runtime.CreateCallback((Action<JsValue>) AcceptAction);
            onReject = Runtime.CreateCallback((Action<JsValue>) RejectAction);

            Then(onAccept, onReject);
            return  tcs.Task;
        }

    }
}
