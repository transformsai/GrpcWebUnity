using System;
using System.Runtime.CompilerServices;

internal static class RuntimeRaw
{

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double GetGlobalObject(out int returnTypeId, string name);


    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeEmptyObject(out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double CreateString(out int returnTypeId, string str);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeArray(out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double CallSlow(out int returnTypeId, double functionRef, double paramArrayRef);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double Call(out int returnTypeId, double functionRef,
        double paramValue1, int paramTypeId1,
        double paramValue2, int paramTypeId2,
        double paramValue3, int paramTypeId3);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double ConstructSlow(out int returnTypeId, double functionRef, double paramArrayRef);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double Construct(out int returnTypeId, double functionRef,
        double paramValue1, int paramTypeId1,
        double paramValue2, int paramTypeId2,
        double paramValue3, int paramTypeId3);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double GetProp(out int returnTypeId, double objectRef, double propNameValue, int propNameTypeId);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double SetProp(out int returnTypeId, double objectRef, double propNameValue, int propNameTypeId, double value, int valueTypeId);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double GetArrayElement(out int returnTypeId, double arrayRef, int index);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double SetArrayElement(out int returnTypeId, double arrayRef, int index, double value, int valueTypeId);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeCallback(out int returnTypeId, double paramArrayRef);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeSharedTypedArray(out int returnTypeId, IntPtr arrayPtr, int typeCode, int arrayLength);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeRemoteTypedArray(out int returnTypeId, IntPtr arrayPtr, int typeCode, int arrayLength);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double CopyFromTypedArray(out int returnTypeId, double typedArrayRef, IntPtr arrayPtr, int typeCode, int arrayLength);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double CopyToTypedArray(out int returnTypeId, double typedArrayRef, IntPtr arrayPtr, int typeCode, int arrayLength);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double GarbageCollect(out int returnTypeId, double value, int typeId);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string GetString(out int returnTypeId, double value, int typeId);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double GetNumber(out int returnTypeId, double value, int typeId);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double Equals(out int returnTypeId, double lhsValue, int lhsType, double rhsValue, int rhsType);
}
