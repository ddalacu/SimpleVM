using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using SimpleVM;

public unsafe class StackBenchmarks
{
    private const int Iterations = 2048 * 10;
    
    [Benchmark()]
    public void StackMethod()
    {
        using var stack = Stack.Create();

        for (int i = 0; i < Iterations; i++)
        {
            stack.Push<int>(1);
            stack.Push<int>(2);

            var b = stack.Pop<int>();
            var a = stack.Pop<int>();

            if (a + b != 3)
                throw new Exception();
        }
    }
    
    [Benchmark()]
    public void Stack2Method()
    {
        using var stack = Stack2.Create();

        for (int i = 0; i < Iterations; i++)
        {
            stack.Push<int>(1);
            stack.Push<int>(2);

            var b = stack.Pop<int>();
            var a = stack.Pop<int>();

            if (a + b != 3)
                throw new Exception();
        }
    }
}

[StructLayout(LayoutKind.Auto)]
public unsafe struct Stack2 : IDisposable
{
    public Word* StackPointer;

    private int _stackSize;


    private Word* _items;


#if DEBUG
    private Type[] _types;
#endif
    
    private const int MaxStackDepth = 512;

    private const int PoolSize = 16;

    private static readonly void*[] _itemsPool = new void*[PoolSize];

    private static int _poolIndex = 0;

    public static Stack2 Create()
    {
        Word* items;

        lock (_itemsPool)
        {
            if (_poolIndex > 0)
            {
                _poolIndex--;
                items = (Word*)_itemsPool[_poolIndex];
            }
            else
            {
                //check if sizeof word is power of 2
                var wordSize = Unsafe.SizeOf<Word>();
                var isPowerOfTwo = (wordSize & (wordSize - 1)) != 0;
                if (isPowerOfTwo)
                    throw new Exception("Word size is not power of 2");

                items = (Word*)Marshal.AllocHGlobal(MaxStackDepth * wordSize);
            }
        }


        return new Stack2
        {
            _items = items,
            _stackSize = MaxStackDepth,
            StackPointer = items,
#if DEBUG
            _types = new Type[MaxStackDepth]
#endif
        };
    }

    public void Dispose()
    {
        if (_items == null)
            return;

        lock (_itemsPool)
        {
            if (_poolIndex >= PoolSize)
            {
                Marshal.FreeHGlobal(new IntPtr(_items));
            }
            else
            {
                _itemsPool[_poolIndex] = _items;
                _poolIndex++;
            }
        }
        
        _items = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushFromStack(Word* from)
    {
        *StackPointer = *from;
#if DEBUG
        _types[StackOffset] = _types[GetOffset(from)];
#endif
        StackPointer++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopDiscard<T>() where T : unmanaged
    {
        var wordCount = Word.GetRequiredWords<T>();
        StackPointer -= wordCount;
    }

    [Pure]
    public int StackOffset
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (int)(StackPointer - _items);
    }

    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetOffset(Word* pos) => (int)(pos - _items);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Push<T>(T value) where T : unmanaged
    {
#if DEBUG
        _types[StackOffset] = typeof(T);
#endif

        var wordCount = Word.GetRequiredWords<T>();

        if (StackOffset + wordCount > _stackSize)
            OverflowException();

        *(T*)StackPointer = value;
        StackPointer += wordCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PushFrom<T>(Word* from) where T : unmanaged
    {
#if DEBUG
        CheckType<T>(from);
        _types[StackOffset] = _types[GetOffset(from)];
#endif

        var wordCount = Word.GetRequiredWords<T>();

        if (StackOffset + wordCount > _stackSize)
            OverflowException();

        *(T*)StackPointer = *(T*)from;
        StackPointer += wordCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Pop<T>() where T : unmanaged
    {
        var wordCount = Word.GetRequiredWords<T>();
        if (StackOffset - wordCount < 0)
            UnderflowException();

        StackPointer -= wordCount;
        
        var value = *(T*)StackPointer;
        CheckStackType<T>();
        return value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (T a, T b) PopTwo<T>() where T : unmanaged
    {
        var wordCount = Word.GetRequiredWords<T>();
        if (StackOffset - (wordCount * 2) < 0)
            UnderflowException();

        StackPointer -= wordCount;
        CheckStackType<T>();
        var b = *(T*)StackPointer;

        StackPointer -= wordCount;
        CheckStackType<T>();
        var a = *(T*)StackPointer;
        return (a, b);
    }

    [Conditional("DEBUG")]
    private void CheckStackType<T>() => CheckType<T>(StackPointer);

    [Conditional("DEBUG")]
    private void CheckType<T>(Word* pos)
    {
#if DEBUG
        if (typeof(T).IsValueType == false)
            throw new Exception($"{typeof(T)} Should be value type");

        var offset = GetOffset(pos);

        var type = _types[offset];

        if (type.IsValueType)
        {
            if (type != typeof(T))
                throw new Exception($"Type mismatch was {type} but expected {typeof(T)}");
        }
        else
        {
            if (typeof(T) != typeof(GCHandle))
                throw new Exception();
        }
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteAt<T>(T value, Word* word) where T : unmanaged
    {
#if DEBUG
        _types[GetOffset(word)] = typeof(T);
#endif

        var wordCount = Word.GetRequiredWords<T>();
        var offset = (int)(word - _items);
        if (offset + wordCount > _stackSize)
            OverflowException();

        *(T*)word = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadFrom<T>(Word* word) where T : unmanaged
    {
        CheckType<T>(word);

        var wordCount = Word.GetRequiredWords<T>();
        var offset = GetOffset(word);
        if (offset + wordCount > _stackSize)
            OverflowException();

        return *(T*)word;
    }

    private static void UnderflowException() => throw new Exception("Stack underflow");

    private static void OverflowException() => throw new Exception("Stack overflow");


#if DEBUG
    public T ReadFromPtr<T>(IntPtr ptr) where T : unmanaged => ReadFrom<T>((Word*)ptr);

    public object GetBoxed(Word* item, Type type)
    {
        if (type.IsValueType)
        {
            var method = typeof(Stack).GetMethod(nameof(ReadFromPtr),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var generic = method.MakeGenericMethod(type);

            return generic.Invoke(this, new object[] { new IntPtr(item) });
        }

        var handle = ReadFrom<GCHandle>(item);

        if (handle != default)
            return handle.Target;

        return null;
    }
#endif
}