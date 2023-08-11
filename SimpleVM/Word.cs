using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace SimpleVM;

public struct Word
{
#if DEBUG
    public ushort Value; //keep word size 2 bytes so we know our code works variable size words
#else
    public IntPtr Value;
#endif

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetRequiredWords<T>()
    {
        var dataSize = Unsafe.SizeOf<T>();
        var wordSize = Unsafe.SizeOf<Word>();
        var wordCount = dataSize / wordSize;
        if (dataSize % wordSize != 0)
            wordCount++;
        return wordCount;
    }

    [Pure]
    public static int GetWordSize(BuiltInType builtInType)
    {
        switch (builtInType)
        {
            case BuiltInType.Bool:
                return GetRequiredWords<bool>();
            case BuiltInType.Byte:
                return GetRequiredWords<byte>();
            case BuiltInType.SByte:
                return GetRequiredWords<sbyte>();
            case BuiltInType.Char:
                return GetRequiredWords<char>();
            case BuiltInType.Decimal:
                return GetRequiredWords<decimal>();
            case BuiltInType.Double:
                return GetRequiredWords<double>();
            case BuiltInType.Float:
                return GetRequiredWords<float>();
            case BuiltInType.Int:
                return GetRequiredWords<int>();
            case BuiltInType.UInt:
                return GetRequiredWords<uint>();
            case BuiltInType.Long:
                return GetRequiredWords<long>();
            case BuiltInType.ULong:
                return GetRequiredWords<ulong>();
            case BuiltInType.Short:
                return GetRequiredWords<short>();
            case BuiltInType.UShort:
                return GetRequiredWords<ushort>();
            case BuiltInType.Object:
                return GetRequiredWords<object>();
            default:
                throw new ArgumentOutOfRangeException(nameof(builtInType), builtInType, null);
        }
    }
}