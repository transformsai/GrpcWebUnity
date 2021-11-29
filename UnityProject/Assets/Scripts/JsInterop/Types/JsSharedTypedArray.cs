using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class JsSharedTypedArray : JsTypedArray
{
    private GCHandle? SharedArrayHandle { get; }

    internal JsSharedTypedArray(double refId, GCHandle sharedArrayHandle) : base(refId, JsTypes.SharedTypedArray)
    {
        SharedArrayHandle = sharedArrayHandle;
    }

    public T[] Access<T>() where T : unmanaged
    {
        if (!SharedArrayHandle.HasValue) throw new ObjectDisposedException("Unable to access handle. Object has likely been disposed");
        var value = SharedArrayHandle.Value.Target;
        return value is T[] target ? target : throw new InvalidCastException($"Type mismatch. Array was of type {value.GetType()} expected {typeof(T)}");
    }

    protected override void Dispose(bool isDisposing)
    {
        SharedArrayHandle?.Free();
        base.Dispose(isDisposing);
        if (!isDisposing) Debug.Fail("Shared array left in Js Memory.");
    }
}
