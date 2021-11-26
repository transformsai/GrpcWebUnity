using System;

public class JsFunction : JsObject
{

    internal JsFunction(double refId, JsTypes typeId = JsTypes.Function) : base(refId, typeId) { }

    
    public virtual JsReference CallSlow(params object[] args)
    {
        var argArray = JsArray.Create(args);
        var retRef = Runtime.CallSlow(RefId, argArray.RefId, out var retType);
        return FromRef(retRef, retType);
    }
    public virtual JsReference Call(JsReference param1 = null, JsReference param2 = null, JsReference param3 = null)
    {
        if (param1 == null) param1 = Undefined;
        if (param2 == null) param2 = Undefined;
        if (param3 == null) param3 = Undefined;

        var refId = Runtime.Call(RefId, 
            (int)param1.TypeId, param1.RefId,
            (int)param2.TypeId, param2.RefId,
            (int)param3.TypeId, param3.RefId,
            out var retType);

        return FromRef(refId, retType);
    }

    public static JsFunction Create(params string[] parametersAndBody) => CreateHostObjectSlow("Function", JsArray.Create(parametersAndBody)) as JsFunction;
}
