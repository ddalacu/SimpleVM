using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace SimpleVM;

public unsafe class CodeBlockWriter : IDisposable
{
    private OPData* _data;

    private int _position;

    private int _capacity;

    public bool Disposed => _data == null;

    public int Capacity => _capacity;

    public uint Position => (uint) _position;

    public CodeBlockWriter(int capacity = 8)
    {
        if (capacity < 8)
            capacity = 8;

        _position = 0;
        _capacity = capacity;

        if (sizeof(OPData) != 4)
            throw new Exception();

        _data = (OPData*) AlignedAllocator.AllocateAligned(capacity * sizeof(OPData), sizeof(OPData));
    }

    public void Add(OPData val)
    {
        if (_position + 1 > _capacity)
            Grow();

        _data[_position] = val;
        _position++;
    }

    // public void Add<T>(T item) where T : unmanaged
    // {
    //     var size = Unsafe.SizeOf<T>();
    //
    //     if (_position + size > _capacity)
    //         Grow();
    //
    //     Unsafe.WriteUnaligned(_data + _position, item);
    //     _position += size;
    // }

    private void Grow()
    {
        var oldCapacity = _capacity;
        _capacity = GetExpanded(oldCapacity);
        var newCode =
            (OPData*) AlignedAllocator.AllocateAligned(_capacity * sizeof(OPData), sizeof(OPData));
        Unsafe.CopyBlock(newCode, _data, (uint) ((uint) oldCapacity * sizeof(OPData)));
        AlignedAllocator.Release((IntPtr) _data);
        _data = newCode;
    }

    [Pure]
    private static int GetExpanded(int oldCapacity) => oldCapacity < 8 ? 8 : oldCapacity * 2;

    public void Dispose()
    {
        AlignedAllocator.Release((IntPtr) _data);
        _data = null;
    }

    public CodeBlock CreateCode()
    {
        if (_data == null)
            throw new ObjectDisposedException(nameof(CodeBlockWriter));

        var byteCount = _position * sizeof(OPData);

        var copy = (byte*) AlignedAllocator.AllocateAligned(byteCount, sizeof(void*));
        Unsafe.CopyBlock(copy, _data, (uint) byteCount);

        return new CodeBlock((OPData*) copy, (int) _position);
    }
}