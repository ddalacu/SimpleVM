using System.Runtime.CompilerServices;

namespace SimpleVMTests;

public unsafe class DataUtils
{
    private static readonly Random Random = new Random();

    public static T CreateRandom<T>() where T : unmanaged
    {
        var count = Unsafe.SizeOf<T>();
        var stackAlloc = stackalloc byte[count];

        for (var i = 0; i < count; i++)
            Unsafe.WriteUnaligned<byte>(stackAlloc + i, (byte) Random.Next(0, 255));

        return Unsafe.ReadUnaligned<T>(stackAlloc);
    }
}