using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class Runtime
{

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double GetGlobalObject(string name, out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double CreateHostObject(string jsClassName,
        int paramTypeId1, double paramRefId1,
        int paramTypeId2, double paramRefId2,
        int paramTypeId3, double paramRefId3,
        out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double CreateHostObjectSlow(string jsClassName, double paramArrayRefId, out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeEmptyObject();

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeString(string str);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeArray();

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double CallSlow(double functionRef, double paramArrayRefId, out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double InvokeSlow(double objectRef, string functionName, double paramArrayRefId, out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double Call(double functionRef,
        int paramTypeId1, double paramRefId1,
        int paramTypeId2, double paramRefId2,
        int paramTypeId3, double paramRefId3,
        out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double Invoke(double objectRef, string functionName,
        int paramTypeId1, double paramRefId1,
        int paramTypeId2, double paramRefId2,
        int paramTypeId3, double paramRefId3,
        out int returnTypeId);


    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double GetProp(double objectRef, string propName, out int returnTypeId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void SetProp(double objectRef, string propName, int valueTypeId, double valueRefId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double GetArrayElement(double arrayRef, int index, out int typeId);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void SetArrayElement(double arrayRef, int index, int valueTypeId, double valueRefId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void GarbageCollect(int typeId, double refId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern string GetString(int typeId, double refId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeCallback(double paramArrayRefId);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeSharedTypedArray(IntPtr arrayPtr, int typeCode, int arrayLength);
    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern double MakeRemoteTypeArray(IntPtr arrayPtr, int typeCode, int arrayLength);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void ReadRemoteArray(double refId, IntPtr arrayPtr, int typeCode, int arrayLength);

    [MethodImpl(MethodImplOptions.InternalCall)]
    internal static extern void UpdateRemoteArray(double refId, IntPtr arrayPtr, int typeCode, int arrayLength);

}
