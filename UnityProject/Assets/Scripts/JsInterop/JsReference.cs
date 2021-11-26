using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;

public abstract class JsReference
{
    public static JsReference Undefined => JsUndefined.Instance;
    public static JsNull Null => JsNull.Instance;
    public static JsBool True => JsBool.True;
    public static JsBool False => JsBool.False;

    internal readonly JsTypes TypeId;
    internal readonly double RefId;

    public abstract object RawValue { get; }
    public abstract bool TruthyValue { get; }

    private protected JsReference(JsTypes typeId, double refId)
    {
        TypeId = typeId;
        RefId = refId;
    }

    public override string ToString() => ReferenceEquals(RawValue, this) ? GetJsStringImpl() : $"{RawValue}";

    public virtual string GetJsStringImpl() => Runtime.GetString((int)TypeId, RefId);
    
    ~JsReference() => Runtime.GarbageCollect((int)TypeId, RefId);

    public JsReference GetProp(string key) => FromRef(Runtime.GetProp(RefId, key, out var typeId), typeId);

    public static JsReference GetGlobalObject(string name) => FromRef(Runtime.GetGlobalObject(name, out var typeId), typeId);
    public static JsReference CreateHostObject(string typeName, JsReference param1 = null, JsReference param2 = null, JsReference param3 = null)
    {
        var refId = Runtime.CreateHostObject(typeName,
            (int)param1.TypeId, param1.RefId,
            (int)param2.TypeId, param2.RefId,
            (int)param3.TypeId, param3.RefId,
            out var typeId);
        return FromRef(refId, typeId);
    }

    public static JsReference CreateHostObjectSlow(string typeName, JsArray parameters) => FromRef(Runtime.CreateHostObjectSlow(typeName, parameters.RefId, out var typeId), typeId);

    public JsReference Invoke(string functionName, JsReference param1 = null, JsReference param2 = null, JsReference param3 = null)
    {
        if (param1 == null) param1 = Undefined;
        if (param2 == null) param2 = Undefined;
        if (param3 == null) param3 = Undefined;

        var refId = Runtime.Invoke(RefId, functionName,
            (int)param1.TypeId, param1.RefId,
            (int)param2.TypeId, param2.RefId,
            (int)param3.TypeId, param3.RefId,
            out var retType);

        return FromRef(refId, retType);
    }



    internal static JsReference FromRef(double refId, int typeId)
    {
        ;
        switch ((JsTypes)typeId)
        {
            case JsTypes.Undefined: return Undefined;
            case JsTypes.Null: return Null;
            case JsTypes.Bool: return refId != 0 ? True : False;
            case JsTypes.Number: return new JsNumber(refId);
            case JsTypes.BigInt: return new JsBigInt(refId);
            case JsTypes.String: return new JsString(refId);
            case JsTypes.Symbol: return new JsSymbol(refId);
            case JsTypes.Object: return new JsObject(refId);
            case JsTypes.Function: return new JsFunction(refId);
            case JsTypes.Callback: return JsCallback.GetReference(refId);
            case JsTypes.Promise: return new JsPromise(refId);
            case JsTypes.Array: return new JsArray(refId);
            case JsTypes.TypedArray: return JsTypedArray.GetReference(refId);
            default: throw new ArgumentOutOfRangeException(nameof(typeId), $"Unknown message type {typeId}");

        }
    }

    public static JsReference FromObject(object obj)
    {
        switch (obj)
        {
            case null: return Null;
            case JsReference i: return i;
            case bool i: return i;
            case int i: return i;
            case float i: return i;
            case double i: return i;
            //Todo: case BigInteger i: return i;
            case string i: return i;
            case IDictionary i: return JsObject.Create(i);
            case IList i: return JsArray.Create(i);
            case Delegate d: return JsCallback.Create(d);
            default: throw new InvalidCastException($"Object cannot be converted to JS");
        }
    }

    public static implicit operator JsReference(int i) => JsNumber.Create(i);
    public static implicit operator JsReference(float i) => JsNumber.Create(i);
    public static implicit operator JsReference(double i) => JsNumber.Create(i);
    public static implicit operator JsReference(bool i) => JsBool.Create(i);
    public static implicit operator JsReference(string i) => JsString.Create(i);
    public static implicit operator JsReference(Array i) => JsArray.Create(i);
    public static implicit operator bool(JsReference i) => i?.TruthyValue ?? false;

}


public static class JsReferenceExtensions
{
    public static T As<T>(this JsReference j)
    {
        if (j is T t) return t;
        return (T)j.ConvertTo(typeof(T));
    }

    public static object ConvertTo(this JsReference j, Type type)
    {
        if (j == null) return null;

        if (type.IsInstanceOfType(j)) return j;

        var value = j.RawValue;

        if (type.IsInstanceOfType(value)) return value;

        var converter = TypeDescriptor.GetConverter(value);

        if (converter.CanConvertTo(type)) return converter.ConvertTo(value, type);

        throw new InvalidCastException($"cannot convert JsReference {j.GetType()} to {type}");
    }
}
