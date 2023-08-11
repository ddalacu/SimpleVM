using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SimpleVM;

[StructLayout(LayoutKind.Auto)]
public ref struct VMThread
{
    private CallFrame[] _frames;

    private int _frameIndex;

    public const int MaxFrames = 512;

    private static ArrayPool<CallFrame> _pool = ArrayPool<CallFrame>.Create(MaxFrames, 12);

    public static VMThread Create()
    {
        return new VMThread()
        {
            _frames = _pool.Rent(MaxFrames),
            _frameIndex = 0,
        };
    }

    public ref CallFrame CurrentFrame => ref _frames[_frameIndex - 1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPushFrame(CallFrame frame)
    {
        if (_frameIndex == _frames.Length)
            return false;

        _frames[_frameIndex] = frame;
        _frameIndex++;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool PopFrame()
    {
        _frameIndex--;
        return _frameIndex > 0;
    }

    public void Dispose()
    {
        _pool.Return(_frames);
        _frames = null;
    }
}