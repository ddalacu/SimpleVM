using System;
using System.Runtime.InteropServices;

namespace SimpleVM;

/// <summary>
/// This is not so fast but we need to be able to use this in net standard
/// </summary>
public unsafe class AlignedAllocator
{
    public static bool IsPowerOfTwo(int x) => (x & (x - 1)) == 0;

    public static IntPtr AllocateAligned(int cb, int alignment)
    {
        if (IsPowerOfTwo(alignment) == false)
            throw new ArgumentException("Block size must be a power of two");

        var size = checked(cb + sizeof(IntPtr) + (alignment - 1));

        var allocated = Marshal.AllocHGlobal(size);

        IntPtr aligned;

        if (sizeof(IntPtr) == 8)
            aligned = (IntPtr) ((allocated.ToInt64() + sizeof(IntPtr) + (alignment - 1)) & ~(alignment - 1));
        else
            aligned = (IntPtr) ((allocated.ToInt32() + sizeof(IntPtr) + (alignment - 1)) & ~(alignment - 1));

        *(((IntPtr*) aligned) - 1) = allocated;

        return aligned;
    }

    public static void Release(IntPtr p)
    {
        if (p == IntPtr.Zero)
            return;

        var original = *(((IntPtr*) p) - 1);
        Marshal.FreeHGlobal(original);
    }
}