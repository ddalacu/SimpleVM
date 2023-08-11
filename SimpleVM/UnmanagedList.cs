using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SimpleVM.Collections;

public unsafe class UnmanagedList<T> : IDisposable where T : unmanaged
{
    private T* _data;

    private int _count;

    private int _capacity;

    public bool Disposed => _data == null;

    public int Capacity => _capacity;

    public ref T this[int index]
    {
        get
        {
#if COLLECTION_SAFETY_CHECKS
            if (index < 0 || index >= _count)
                throw new IndexOutOfRangeException();
#endif

            return ref _data[index];
        }
    }

    public int Count => _count;

    public UnmanagedList(int capacity = 8)
    {
        if (capacity < 8)
            capacity = 8;

        _count = 0;
        _capacity = capacity;
        _data = (T*) Marshal.AllocHGlobal(_capacity * sizeof(T));
    }

    public int Add(T val)
    {
        if (_count == _capacity)
        {
            var oldCapacity = _capacity;
            _capacity = GrowCapacity(oldCapacity);
            var newCode = (T*) Marshal.AllocHGlobal(_capacity * sizeof(T));
            Unsafe.CopyBlock(newCode, _data, (uint) ((uint) oldCapacity * sizeof(T)));
            Marshal.FreeHGlobal((IntPtr) _data);
            _data = newCode;
        }

        _data[_count] = val;
        _count++;
        return _count - 1;
    }

    public Span<T> Span(int offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > _count)
            throw new IndexOutOfRangeException();

        return new Span<T>(_data + offset, count);
    }

    public Span<TC> Casted<TC>(int offset, int count) where TC : unmanaged
    {
        if (Unsafe.SizeOf<TC>() != Unsafe.SizeOf<T>())
            throw new Exception("Size of types must match!");

        if (offset < 0 || count < 0 || offset + count > _count)
            throw new IndexOutOfRangeException();

        return new Span<TC>(_data + offset, count);
    }

    [Pure]
    private static int GrowCapacity(int oldCapacity) => oldCapacity < 8 ? 8 : oldCapacity * 2;

    public void Dispose()
    {
        fixed (void* casted = &_data)
        {
            ref var ptr = ref Unsafe.AsRef<IntPtr>(casted);

            var old = Interlocked.Exchange(ref ptr, IntPtr.Zero);

            if (old != IntPtr.Zero)
                Marshal.FreeHGlobal((IntPtr) _data);
        }
    }
}