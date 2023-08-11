using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SimpleVM.Collections;

public enum ConstantPosition : ushort
{
}

/// <summary>
/// Class used to write data to a buffer
/// </summary>
public sealed unsafe class ConstantsBufferWriter : IDisposable
{
    //we use a managed array increase safety since we care more about script execution performance than compiler performance 
    //in case we need to increase performance we can use a native array
    private byte[] _buffer;

    private uint _position;

    private List<ConstantPosition> _handlePositions = new List<ConstantPosition>();

    public ConstantsBufferWriter(int capacity)
    {
        _buffer = new byte[capacity];
        _position = 0;
    }

    public void EnsureNext(uint count)
    {
        var size = _position + count;

        if (size <= _buffer.Length)
            return;

        var capacity = _buffer.Length * 2;

        var expanded = new byte[capacity];
        Array.Copy(_buffer, expanded, _position);
        _buffer = expanded;
    }

    public ConstantPosition Write<T>(T value)
    {
        AlignData<T>();
        EnsureNext((uint) Unsafe.SizeOf<T>());

        var position = checked((ConstantPosition) _position);

        if (typeof(T).IsValueType)
        {
            fixed (byte* ptr = _buffer)
                Unsafe.Write(ptr + _position, value);
            _position += (uint) Unsafe.SizeOf<T>();
        }
        else
        {
            if (sizeof(GCHandle) != Unsafe.SizeOf<T>())
                throw new Exception("WTF");

            GCHandle handle;
            
            if (value == null)
            {
                handle = default;
            }
            else
            {
                handle = GCHandle.Alloc(value, GCHandleType.Normal);
                _handlePositions.Add(position);
            }
            
            fixed (byte* ptr = _buffer)
                Unsafe.Write(ptr + _position, handle);
            _position += (uint) Unsafe.SizeOf<GCHandle>();
        }

        return position;
    }

    private void AlignData<T>()
    {
        var dataSize = Unsafe.SizeOf<T>();
        if (_position % dataSize == 0)
            return;

        var additional = dataSize - (_position % dataSize);
        _position += (uint) additional;
    }

    public ConstantsBufferReader CreateBuffer()
    {
        if (_handlePositions == null)
            throw new Exception("Already disposed");

        var toReturn = new ConstantsBufferReader(_buffer, _position, _handlePositions.ToArray());
        _handlePositions = null;
        Dispose();
        return toReturn;
    }

    private void InternalDispose()
    {
        if (_handlePositions == null)
            return;

        foreach (var handlePos in _handlePositions)
        {
            fixed (byte* ptr = _buffer)
            {
                var handle = Unsafe.Read<GCHandle>(ptr + (uint) handlePos);
                handle.Free();
            }
        }

        _handlePositions = null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        InternalDispose();
    }

    ~ConstantsBufferWriter()
    {
        InternalDispose();
    }
}