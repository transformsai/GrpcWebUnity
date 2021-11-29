using System;

public class JsTypedArray : JsObject, IDisposable
{

    public int Length => GetProp("length").As<int>();
    internal JsTypedArray(double refId, JsTypes typeId = JsTypes.TypedArray) : base(refId, typeId) { }

    public virtual T[] GetDataCopy<T>() where T : unmanaged
    {
        var array = new T[Length];
        Runtime.CopyFromTypedArray(this, array);
        return array;
    }


    public virtual void SetDataCopy<T>(T[] newValuesArray) where T : unmanaged =>
        Runtime.CopyToTypedArray(this, newValuesArray);


    public static TypedArrayTypeCode GetTypeCode(Array t)
    {
        if (t is float[]) return TypedArrayTypeCode.Float32Array;
        if (t is double[]) return TypedArrayTypeCode.Float64Array;
        if (t is short[]) return TypedArrayTypeCode.Int16Array;
        if (t is int[]) return TypedArrayTypeCode.Int32Array;
        if (t is sbyte[]) return TypedArrayTypeCode.Int8Array;
        if (t is ushort[]) return TypedArrayTypeCode.Uint16Array;
        if (t is uint[]) return TypedArrayTypeCode.Uint32Array;
        if (t is byte[]) return TypedArrayTypeCode.Uint8Array;
        throw new InvalidCastException("Unsupported TypedArray");
    }


}

public enum TypedArrayTypeCode
{
    Int8Array = 5,
    Uint8Array = 6,
    Int16Array = 7,
    Uint16Array = 8,
    Int32Array = 9,
    Uint32Array = 10,
    Float32Array = 13,
    Float64Array = 14,
    Uint8ClampedArray = 0xF,
}
