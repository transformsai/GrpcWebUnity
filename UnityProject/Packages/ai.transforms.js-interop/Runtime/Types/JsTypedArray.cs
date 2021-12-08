using System;
using JsInterop.Internal;

namespace JsInterop.Types
{
    public class JsTypedArray : JsObject
    {

        private int? _length = 0;
        public virtual int Length => _length ?? (_length = GetProp("length").As<int>()).Value;

        internal JsTypedArray(double refId, JsTypes typeId = JsTypes.TypedArray) : base(refId, typeId) { }

        public T[] GetDataCopy<T>() where T : unmanaged
        {
            var copyDestination = new T[Length];
            GetDataCopy<T>(copyDestination);
            return copyDestination;
        }

        public virtual void GetDataCopy<T>(T[] copyDestination) where T : unmanaged
        {
            CheckLengths(Length, copyDestination.Length);
            using var dest = Runtime.CreateSharedTypedArray(copyDestination);
            dest.Invoke("set", this);

        }

        public virtual void SetDataCopy<T>(T[] copySource) where T : unmanaged
        {
            CheckLengths(copySource.Length, Length);
            using var src = Runtime.CreateSharedTypedArray(copySource);
            Invoke("set", src);
        }

        protected static void CheckLengths(int src, int dest)
        {
            if (src > dest) throw new ArgumentException("destination is smaller than source");
        }

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


//the name of these need to match the constructor of the JS counterpart
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
}
