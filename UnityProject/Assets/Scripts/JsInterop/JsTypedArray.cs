using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

public class JsTypedArray : JsObject, IDisposable
{
    public static Dictionary<double, JsTypedArray> SharedArrayRegistry = new Dictionary<double, JsTypedArray>();

    private GCHandle? SharedHandle { get; set; }
    private bool IsShared { get; }

    private JsTypedArray(double refId, GCHandle? sharedHandle = null) : base(refId, JsTypes.TypedArray)
    {
        SharedHandle = sharedHandle ?? default;
        IsShared = sharedHandle.HasValue;
        if (IsShared) SharedArrayRegistry[refId] = this;
    }



    public static JsTypedArray GetReference(double refId) =>
        SharedArrayRegistry.TryGetValue(refId, out var value) ? value : new JsTypedArray(refId);

    public static JsTypedArray CreateShared<T>(T[] array) where T : unmanaged
    {
        var typeCode = GetTypeCode(array);
        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        try
        {
            var ptr = GCHandle.ToIntPtr(handle);
            var refId = Runtime.MakeSharedTypedArray(ptr, (int)typeCode, array.Length);
            return new JsTypedArray(refId, handle);
        }
        catch (Exception)
        {
            handle.Free();
            throw;
        }
    }

    public static JsTypedArray CreateRemoteCopy<T>(T[] array) where T : unmanaged
    {
        var typeCode = GetTypeCode(array);
        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        try
        {
            var ptr = GCHandle.ToIntPtr(handle);
            var refId = Runtime.MakeRemoteTypeArray(ptr, (int)typeCode, array.Length);
            return new JsTypedArray(refId);
        }
        finally
        {
            handle.Free();
        }
    }

    public T[] Access<T>() where T : unmanaged
    {
        if (!IsShared) throw new InvalidOperationException($"You may not access remote arrays. Use {nameof(GetDataCopy)} instead");
        if (!SharedHandle.HasValue) throw new ObjectDisposedException("Unable to access handle. Object has likely been disposed");
        var value = SharedHandle.Value.Target;
        return value is T[] target ? target : throw new InvalidCastException($"Type mismatch. Array was of type {value.GetType()} expected {typeof(T)}");
    }

    public T[] GetDataCopy<T>() where T : unmanaged
    {
        if (IsShared)
        {
            var rawArray = Access<T>();
            return rawArray.ToArray();
        }

        var length = GetProp("length").As<int>();

        var array = new T[length];

        var typeCode = GetTypeCode(array);
        var handle = GCHandle.Alloc(array, GCHandleType.Pinned);
        try
        {
            var ptr = GCHandle.ToIntPtr(handle);
            Runtime.ReadRemoteArray(RefId, ptr, (int)typeCode, array.Length);
            return array;
        }
        finally
        {
            handle.Free();
        }
    }


    public void SetDataCopy<T>(T[] newValuesArray) where T : unmanaged
    {
        if (IsShared)
        {
            var rawArray = Access<T>();
            if (rawArray == newValuesArray) return;
            if (newValuesArray.Length != rawArray.Length) throw new IndexOutOfRangeException("Array size mismatch");
            Array.Copy(newValuesArray, rawArray, newValuesArray.Length);
        }

        var length = GetProp("length").As<int>();

        if (newValuesArray.Length != length) throw new IndexOutOfRangeException("Array size mismatch");

        var typeCode = GetTypeCode(newValuesArray);
        var handle = GCHandle.Alloc(newValuesArray, GCHandleType.Pinned);
        try
        {
            var ptr = GCHandle.ToIntPtr(handle);
            Runtime.UpdateRemoteArray(RefId, ptr, (int)typeCode, newValuesArray.Length);
        }
        finally
        {
            handle.Free();
        }
    }

    public void Dispose()
    {
        if (!IsShared) return;
        SharedHandle?.Free();
        SharedArrayRegistry.Remove(RefId);
    }

    public static TypedArrayTypeCode GetTypeCode(Array t)
    {
        switch (t)
        {
            case float[]: return TypedArrayTypeCode.Float32Array;
            case double[]: return TypedArrayTypeCode.Float64Array;
            case short[]: return TypedArrayTypeCode.Int16Array;
            case int[]: return TypedArrayTypeCode.Int32Array;
            case sbyte[]: return TypedArrayTypeCode.Int8Array;
            case ushort[]: return TypedArrayTypeCode.Int16Array;
            case uint[]: return TypedArrayTypeCode.Uint32Array;
            case byte[]: return TypedArrayTypeCode.Uint8Array;
            default: throw new InvalidCastException("Unsupported TypedArray");
        }
    }

}
