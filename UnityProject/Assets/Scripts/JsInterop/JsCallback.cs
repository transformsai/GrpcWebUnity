using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public class JsCallback : JsFunction, IDisposable
{
    private static Dictionary<double, JsCallback> CallbackRegistry { get; set; }

    private Delegate Delegate { get; }
    private bool AutoDispose { get; }


    private JsCallback(double refId, Delegate callback, bool autoDispose = false) : base(refId, JsTypes.Callback)
    {
        Delegate = callback;
        AutoDispose = autoDispose;
        CallbackRegistry[refId] = this;
    }


    internal static JsCallback GetReference(double refId) =>
        CallbackRegistry.TryGetValue(refId, out var value) ? value :
            throw new KeyNotFoundException("Callback not found, Likely disposed already.");

    public static JsCallback CreateOneShot(Delegate del) => Create(del, true);
    public static JsCallback Create(Delegate del, bool disposeAfterCallback = false)
    {
        var parameters = del.Method.GetParameters();
        var fnArgs = parameters.Select(it => it.Name);
        var paramArray = JsArray.Create(fnArgs);
        var callbackId = (int)Runtime.MakeCallback(paramArray.RefId);
        return new JsCallback(callbackId, del, disposeAfterCallback);
    }



    public void Callback(JsArray parameters)
    {
        var paramList = Delegate.Method.GetParameters();
        var argArray = new object[paramList.Length];
        for (var i = 0; i < paramList.Length; i++)
        {
            var paramType = paramList[i].ParameterType;
            var argument = parameters.Count > i ? parameters[i] : Undefined;
            argArray[i] = argument.ConvertTo(paramType);
        }

        try
        {
            Delegate.DynamicInvoke(argArray);
        }
        finally
        {
            if (AutoDispose) Dispose();
        }
    }


    public void Dispose()
    {
        CallbackRegistry.Remove((int)RefId);
    }
}
