using System;

namespace SimpleVM;

//All c# built-in types signed and unsigned
public enum BuiltInType : byte
{
    Bool,
    Byte,
    SByte,
    Char,
    Decimal,
    Double,
    Float,
    Int,
    UInt,
    Long,
    ULong,
    Short,
    UShort,
    Object,
}

public static class BuiltInTypeUtils
{
    public static BuiltInType GetBuiltInType<T>()
        => GetBuiltInType(typeof(T));

    public static BuiltInType GetBuiltInType(Type type)
    {
        if (type == typeof(bool))
            return BuiltInType.Bool;
        if (type == typeof(byte))
            return BuiltInType.Byte;
        if (type == typeof(sbyte))
            return BuiltInType.SByte;
        if (type == typeof(char))
            return BuiltInType.Char;
        if (type == typeof(decimal))
            return BuiltInType.Decimal;
        if (type == typeof(double))
            return BuiltInType.Double;
        if (type == typeof(float))
            return BuiltInType.Float;
        if (type == typeof(int))
            return BuiltInType.Int;
        if (type == typeof(uint))
            return BuiltInType.UInt;
        if (type == typeof(long))
            return BuiltInType.Long;
        if (type == typeof(ulong))
            return BuiltInType.ULong;
        if (type == typeof(short))
            return BuiltInType.Short;
        if (type == typeof(ushort))
            return BuiltInType.UShort;
        if (type.IsValueType == false)
            return BuiltInType.Object;  

        throw new Exception($"Unknown type {type}");
    }
}