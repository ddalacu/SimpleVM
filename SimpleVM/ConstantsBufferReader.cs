using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SimpleVM.Collections;

/// <summary>
/// Unmanaged buffer, will dispose of the buffer when disposed
/// </summary>
[StructLayout(LayoutKind.Auto)]
public unsafe struct ConstantsBufferReader : IDisposable
{
    private byte* _buffer;
    public uint Length { get; }

    private ConstantPosition[] _handlePositions;

    public bool IsValid => _buffer != null;

    public ConstantsBufferReader(byte[] original, uint length, ConstantPosition[] handlePositions)
    {
        var copy = (byte*) Marshal.AllocHGlobal((int) length).ToPointer();

        fixed (byte* ptr = original)
            Unsafe.CopyBlock(copy, ptr, (uint) length);

        _buffer = copy;
        Length = length;
        _handlePositions = handlePositions;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>(ConstantPosition position)
    {
        if ((ushort) position + Unsafe.SizeOf<T>() > Length)
            throw new IndexOutOfRangeException();

        CheckAlignment<T>(position);

        return Unsafe.Read<T>(_buffer + (uint) position);
    }

    public T ReadObj<T>(ConstantPosition position) where T : class
    {
        var handle = Read<GCHandle>(position);

        if (handle == default)
            return null;

#if DEBUG
        if (handle.IsAllocated == false)
            throw new InvalidDataException($"Invalid handle at position {position}");
#endif

        return Unsafe.As<T>(handle.Target);
    }

    private void CheckAlignment<T>(ConstantPosition position)
    {
        if ((ushort) position % Unsafe.SizeOf<T>() == 0)
            return;

        throw new InvalidDataException("Buffer is not aligned");
    }

    public void Dispose()
    {
        if (_handlePositions != null)
        {
            foreach (var handlePosition in _handlePositions)
            {
                var handle = Read<GCHandle>(handlePosition);
                handle.Free();
            }

            _handlePositions = null;
        }

        if (_buffer == null)
            return;

        Marshal.FreeHGlobal(new IntPtr(_buffer));
        _buffer = null;
    }
}