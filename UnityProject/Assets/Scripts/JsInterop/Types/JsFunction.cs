using System;
using System.Runtime.CompilerServices;

public class JsFunction : JsObject
{

    private static readonly ConditionalWeakTable<string, JsFunction> FunctionCache = new ConditionalWeakTable<string, JsFunction>();
    internal JsFunction(double refId, JsTypes typeId = JsTypes.Function) : base(refId, typeId) { }


    public JsFunction Bind(JsValue thisObj) => Invoke("Bind", thisObj).As<JsFunction>();

    public JsValue Construct(JsValue[] values) => Runtime.Construct(this, values);
    public JsValue Construct(JsValue param1 = default, JsValue param2 = default, JsValue param3 = default) => Runtime.Construct(this, param1, param2, param3);
    public JsValue Call(params JsValue[] values) => Runtime.Call(this, values);
    public JsValue Call(JsValue param1 = default, JsValue param2 = default, JsValue param3 = default) => Runtime.Call(this, param1, param2, param3);

    public static bool TryGetFunction(string str, out JsFunction jsFunction) => FunctionCache.TryGetValue(str, out jsFunction);
    public static void StoreFunction(string str, JsFunction jsFunction)
    {
        if (jsFunction == null) throw new NullReferenceException("Tried to store null reference");
        FunctionCache.Add(str, jsFunction);
    }

}
