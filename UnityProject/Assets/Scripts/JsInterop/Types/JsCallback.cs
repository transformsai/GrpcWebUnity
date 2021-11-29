using System;
using System.Runtime.InteropServices;

public class JsCallback : JsFunction
{
    private Delegate Delegate { get; }
    private GCHandle? _jsHandle;

    internal JsCallback(double refId, Delegate callback) : base(refId, JsTypes.Callback)
    {
        Delegate = callback;
        AcquireFromJs();
    }

    public void Callback(JsArray parameters)
    {
        var paramList = Delegate.Method.GetParameters();
        var argArray = new object[paramList.Length];
        for (var i = 0; i < paramList.Length; i++)
        {
            var paramType = paramList[i].ParameterType;
            var argument = parameters.Count > i ? parameters[i] : JsValue.Undefined;
            argArray[i] = argument.As(paramType);
        }
        Delegate.DynamicInvoke(argArray);
    }


    // Used so that this object doesn't get GC'd if it's only referenced from JS
    // This allows you to create long-lived callbacks without worrying about their life-cycles
    internal void AcquireFromJs()
    {
        if (_jsHandle != null) throw new InvalidOperationException($"{nameof(JsCallback)} acquired while already held.");
        _jsHandle = GCHandle.Alloc(this, GCHandleType.Normal);
    }

    internal void ReleaseFromJs()
    {
        _jsHandle?.Free();
        _jsHandle = null;
    }

    protected override void Dispose(bool isDisposing)
    {
        ReleaseFromJs();
        base.Dispose(isDisposing);
    }
}
