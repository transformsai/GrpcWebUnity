internal enum JsTypes
{
    Undefined = -1,
    Null = 0,
    Bool = 1,
    Number = 2,
    BigInt = 3,
    String = 4,
    Symbol = 5,
    Object = 100,
    Function = 200,
    Callback = 201,
    Promise = 202,
    Array = 300,
    TypedArray = 400,
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
