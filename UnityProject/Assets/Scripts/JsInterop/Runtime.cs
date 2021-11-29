using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Raw = RuntimeRaw;

public static class Runtime
{
    // --- Api ---
    public static JsValue GetGlobalValue(string identifier) =>
        Raw.GetGlobalObject(out var typeId, identifier).Receive(typeId);
    public static JsValue CreateHostObject(string identifier, JsValue param1 = default, JsValue param2 = default, JsValue param3 = default) =>
        GetGlobalValue(identifier).As<JsFunction>().Construct(param1, param2, param3);
    public static JsValue CreateHostObject(string identifier, params JsValue[] args) =>
        GetGlobalValue(identifier).As<JsFunction>().Construct(args);
    public static JsObject CreateObject() => Raw.MakeEmptyObject(out var typeId).Receive(typeId).As<JsObject>();

    public static JsObject CreateObject(IDictionary dictionary)
    {
        var obj = CreateObject();
        obj.Populate(dictionary);
        return obj;
    }

    public static JsString CreateString(string str)
    {
        if (JsString.TryGetString(str, out var jsString)) return jsString;
        var refId = Raw.CreateString(out var typeId, str);
        CheckException(refId, typeId);
        var jsStr = new JsString(refId, str);
        return jsStr;
    }

    public static JsBigInt CreateBigInt(BigInteger bigInt) => GetGlobalValue("BigInt").As<JsFunction>().Call(bigInt.ToString()).As<JsBigInt>();

    public static JsArray CreateArray() => Raw.MakeArray(out var typeId).Receive(typeId).As<JsArray>();

    public static JsArray CreateArray(IEnumerable list)
    {
        var arr = CreateArray();
        foreach (var obj in list) arr.Add(CreateFromObject(obj));
        return arr;
    }

    public static JsFunction CreateFunction(string body, bool cache = false)
    {
        if (cache && JsFunction.TryGetFunction(body, out var fn)) return fn;
        fn = GetGlobalValue("Function").As<JsFunction>().Call(body).As<JsFunction>();
        if(cache) JsFunction.StoreFunction(body, fn);
        return fn;
    }

    public static JsFunction CreateFunction(params string[] parametersThenBody)
    {
        var args = parametersThenBody.Select(it => (JsValue)it).ToArray();
        return GetGlobalValue("Function").As<JsFunction>().Construct(args).As<JsFunction>();
    }


    public static JsValue Construct(JsFunction function, JsValue param1 = default, JsValue param2 = default, JsValue param3 = default)
    {
        var refId = Raw.Construct(out var typeId1, function.Ref(), param1.Ref(),
            param1.Type(), param2.Ref(),
            param2.Type(), param3.Ref(), param3.Type());
        return Receive(refId, typeId1);

    }
    public static JsValue Construct(JsFunction function, params JsValue[] p) =>
        Raw.ConstructSlow(out var typeId, function.Ref(), CreateArray(p).Ref()).Receive(typeId);

    public static JsValue Call(JsFunction function, JsValue param1 = default, JsValue param2 = default, JsValue param3 = default)
    {
        var refId = Raw.Call(out var typeId1, function.Ref(), param1.Ref(),
            param1.Type(), param2.Ref(),
            param2.Type(), param3.Ref(), param3.Type());
        return Receive(refId, typeId1);
    }

    public static JsValue Call(JsFunction function, params JsValue[] p) =>
        Raw.CallSlow(out var typeId, function.Ref(), CreateArray(p).Ref()).Receive(typeId);

    public static JsValue GetProp(JsValue obj, JsValue propName) =>
        Raw.GetProp(out var typeId, obj.Value, propName.Ref(), propName.Type()).Receive(typeId);
    public static void SetProp(JsObject obj, JsValue propName, JsValue value) =>
        Raw.SetProp(out var typeId, obj.Ref(), propName.Ref(), propName.Type(), value.Ref(), value.Type()).CheckException(typeId);
    public static JsValue GetArrayElement(JsArray array, int index) =>
        Raw.GetArrayElement(out var typeId, array.Ref(), index).Receive(typeId);
    public static void SetArrayElement(JsArray array, int index, JsValue value) =>
        Raw.SetArrayElement(out var typeId, array.Ref(), index, value.Ref(), value.Type()).CheckException(typeId);
    public static JsCallback CreateCallback(Delegate del)
    {
        var parameters = del.Method.GetParameters();
        var fnArgs = parameters.Select(it => it.Name);
        var paramArray = CreateArray(fnArgs);
        var refId = Raw.MakeCallback(out var typeId, paramArray.Ref());
        CheckException(refId, typeId);
        return new JsCallback(refId, del);
    }

    internal static JsSharedTypedArray MakeSharedTypedArray<T>(T[] array) where T : unmanaged
    {
        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        try
        {
            var ptr = GCHandle.ToIntPtr(handle);
            var refId = Raw.MakeSharedTypedArray(out var typeId, ptr, array.TypeCode(), array.Length);
            CheckException(refId, typeId);
            return new JsSharedTypedArray(refId, handle);
        }
        catch (Exception)
        {
            handle.Free();
            throw;
        }
    }


    public static JsTypedArray CreateTypedArray<T>(T[] array) where T : unmanaged
    {
        return WithHandle(array, arrayPtr =>
        {
            var refId = Raw.MakeRemoteTypedArray(out var typeId, arrayPtr, array.TypeCode(), array.Length);
            CheckException(refId, typeId);
            return new JsTypedArray(refId);
        });
    }

    internal static void CopyFromTypedArray<T>(JsTypedArray remoteArray, T[] copyDestination) where T : unmanaged =>
        WithHandle(copyDestination, arrayPtr =>
            Raw.CopyFromTypedArray(out var typeId, remoteArray.Ref(), arrayPtr, copyDestination.TypeCode(), copyDestination.Length)
                .CheckException(typeId));
    internal static void CopyToTypedArray<T>(JsTypedArray remoteArray, T[] copySource) where T : unmanaged =>
        WithHandle(copySource, arrayPtr =>
        Raw.CopyToTypedArray(out var typeId, remoteArray.Ref(), arrayPtr, copySource.TypeCode(), copySource.Length)
            .CheckException(typeId));
    internal static string GetString(JsValue obj)
    {
        var str = Raw.GetString(out var typeId, obj.Ref(), obj.Type());
        CheckException(0, typeId);
        return str;
    }

    public static double GetNumber(JsValue obj)
    {
        var num = Raw.GetNumber(out var typeId, obj.Ref(), obj.Type());
        CheckException(num, typeId);
        return num;
    }

    public static bool Equals(JsValue lhs, JsValue rhs) =>
        Raw.Equals(out var typeId, lhs.Ref(), lhs.Type(), rhs.Ref(), rhs.Type()).Receive(typeId);

    internal static void GarbageCollect(JsReference obj) =>
        Raw.GarbageCollect(out var typeId, obj.Ref(), obj.Type()).Receive(typeId);

    // --- Utilities ---


    private static double Ref(this JsValue js) => js.Value;
    private static double Ref(this JsReference js) => js?.RefValue.Ref() ?? JsValue.Null.Ref();
    private static int Type(this JsValue js) => (int)js.TypeId;
    private static int Type(this JsReference js) => js?.RefValue.Type() ?? JsValue.Null.Type();
    private static int TypeCode(this Array array) => (int)JsTypedArray.GetTypeCode(array);

    private static T WithHandle<T>(Array t, Func<IntPtr, T> func)
    {
        var handle = GCHandle.Alloc(t, GCHandleType.Pinned);
        try { return func(handle.AddrOfPinnedObject()); }
        finally { handle.Free(); }
    }

    private static void WithHandle(Array t, Action<IntPtr> func)
    {
        var handle = GCHandle.Alloc(t, GCHandleType.Pinned);
        try { func(handle.AddrOfPinnedObject()); }
        finally { handle.Free(); }
    }


    private static void CheckException(this double refId, int typeId, string detail = null)
    {
        if (typeId != (int)JsTypes.Exception) return;
        if (refId == 0) throw new JsException("Unknown Js Exception:" + detail);
        var exceptionString = Raw.GetString(out var strType, refId, (int)JsTypes.String);
        if (strType != (int)JsTypes.String) throw new JsException("Unknown Js Exception:" + detail);
        if (detail != null) exceptionString = $"{detail}:\n{exceptionString}";
        throw new JsException(exceptionString);
    }

    private static JsValue Receive(this double refId, int typeId)
    {
        CheckException(refId, typeId);
        var type = (JsTypes)typeId;


        if (!type.IsValueType() && JsReference.TryGetRef(refId, out var reference))
            return reference;

        switch (type)
        {
            // Values
            case JsTypes.Undefined: return JsValue.Undefined;
            case JsTypes.Null: return JsValue.Null;
            case JsTypes.Bool: return refId != 0 ? JsValue.True : JsValue.False;
            case JsTypes.Number: return refId;
            // References
            case JsTypes.BigInt: return new JsBigInt(refId);
            case JsTypes.String: return new JsString(refId);
            case JsTypes.Symbol: return new JsSymbol(refId);
            case JsTypes.Object: return new JsObject(refId);
            case JsTypes.Function: return new JsFunction(refId);
            case JsTypes.Promise: return new JsPromise(refId);
            case JsTypes.Array: return new JsArray(refId);
            // Cannot create these from just reference numbers. These need to be made in C#
            case JsTypes.Callback:
            case JsTypes.SharedTypedArray:
                throw new ObjectDisposedException($"Received JS reference of special type {typeId}. This object was likely disposed, but was still referenced by JS.");
            default:
                throw new ArgumentOutOfRangeException(nameof(typeId), $"Unknown message type {typeId}");

        }
    }

    public static JsValue CreateFromObject(object obj)
    {
        switch (obj)
        {
            case null: return null;
            case JsValue i: return i;
            case bool i: return i;
            case int i: return i;
            case float i: return i;
            case double i: return i;
            case BigInteger i: return i;
            case string i: return i;
            case Delegate d: return CreateCallback(d);
            case IDictionary i: return CreateObject(i);
            case IEnumerable i: return CreateArray(i);
            default: throw new InvalidCastException($"Object cannot be converted to {nameof(JsValue)}");
        }
    }

}
