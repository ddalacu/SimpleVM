using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;


#if DEBUG
using System.Collections.Concurrent;
using System.Reflection;
#endif

namespace SimpleVM;

[StructLayout(LayoutKind.Auto)]
public unsafe ref struct Stack
{
    public Word* StackPointer;

    private int _stackSize;

    private Word* _items;

#if DEBUG
    private Type[] _types;
#endif

    private const int MaxStackDepth = 1024;

    private const int PoolSize = 16;

    private static readonly void*[] _itemsPool = new void*[PoolSize];

    private static int _poolIndex = 0;

    public static Stack Create()
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


        return new Stack
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
    public void Push<T>(T value)
    {
#if DEBUG
        _types[StackOffset] = typeof(T);
#endif

        var wordCount = Word.GetRequiredWords<T>();

        if (StackOffset + wordCount > _stackSize)
            OverflowException();

        *(T*)StackPointer = value;
        //Unsafe.Write(StackPointer,value);
        StackPointer += wordCount;
    }

    public void SafePush<T>(T value)
    {
        if (typeof(T).IsValueType)
        {
            Push<T>(value);
        }
        else
        {
#if DEBUG
            Console.WriteLine($"Allocated handle for {value}");
#endif
            Push(GCHandle.Alloc(value));
        }
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
    public T Pop<T>()
    {
        var wordCount = Word.GetRequiredWords<T>();
        if (StackOffset - wordCount < 0)
            UnderflowException();

        StackPointer -= wordCount;

        var value = Unsafe.Read<T>(StackPointer);
        CheckStackType<T>();
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T SafePop<T>()
    {
        if (typeof(T).IsValueType)
            return Pop<T>();

        var handle = Pop<GCHandle>();

        if (handle.IsAllocated == false)
            return default;

        var value = (T)handle.Target;

#if DEBUG
        Console.WriteLine("Releasing handle for {0}", value);
#endif

        handle.Free();

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
    public static object ReadFromPtr<T>(ref Stack stack, Word* ptr) where T : unmanaged =>
        stack.ReadFrom<T>(ptr);

    private delegate object ReadDelegate(ref Stack stack, Word* ptr);

    private static ConcurrentDictionary<Type, ReadDelegate> _readers = new ConcurrentDictionary<Type, ReadDelegate>();

    private static ReadDelegate GetReader(Type type)
    {
        var method = typeof(Stack).GetMethod(nameof(ReadFromPtr),
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        var generic = method.MakeGenericMethod(type);

        return (ReadDelegate)Delegate.CreateDelegate(typeof(ReadDelegate), generic);
    }
    
    public object GetBoxed(Word* item, Type type)
    {
        if (type.IsValueType)
        {
            if (_readers.TryGetValue(type, out var reader))
                return reader(ref this, item);

            reader = GetReader(type);
            _readers.TryAdd(type, reader);
            
            return reader(ref this, item);
        }

        var handle = ReadFrom<GCHandle>(item);

        if (handle != default)
            return handle.Target;

        return null;
    }
#endif
}