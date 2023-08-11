using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SimpleVM;

[StructLayout(LayoutKind.Auto)]
public unsafe struct CodeBlock : IDisposable
{
    public OPData* Pointer;

    public readonly int Length;

    public CodeBlock(OPData* pointer, int length)
    {
        if (pointer == null)
            throw new ArgumentNullException(nameof(pointer));

        Pointer = pointer;
        Length = length;
    }

    public T ReadUnaligned<T>(int index) where T : unmanaged
    {
        if (index + Unsafe.SizeOf<T>() > Length)
            throw new IndexOutOfRangeException();

        return Unsafe.ReadUnaligned<T>(Pointer + index);
    }
    
    public OPData this[int index] => Pointer[index];
    
    public bool IsValid => Pointer != null;

    public void Dispose()
    {
        if (Pointer == null)
            return;

        AlignedAllocator.Release((IntPtr) Pointer);
        Pointer = null;
    }
}