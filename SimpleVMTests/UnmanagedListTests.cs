using System.Reflection;
using System.Runtime.CompilerServices;
using SimpleVM;
using SimpleVM.Collections;

namespace SimpleVMTests;

public unsafe class UnmanagedListTests
{
    private struct MyStruct
    {
        public int A;
        public int B;
    }

    [TestCase<int>()]
    [TestCase<double>()]
    [TestCase<MyStruct>()]
    public void InsertSuccessfully<T>() where T : unmanaged
    {
        using var list = new UnmanagedList<T>();

        list.Add(default);

        Assert.That(list.Count, Is.EqualTo(1));
    }

    [Test]
    public void InsertWorks()
    {
        using var list = new UnmanagedList<int>(1024);

        for (var i = 0; i < 1024; i++)
            list.Add(i);

        for (var i = 0; i < 1024; i++)
            Assert.That(list[i], Is.EqualTo(i));
    }

    [TestCase<int>()]
    [TestCase<double>()]
    [TestCase<MyStruct>()]
    public void DisposedSuccessfully<T>() where T : unmanaged
    {
        var list = new UnmanagedList<T>();

        list.Add(default);

        list.Dispose();

        Assert.That(list.Disposed, Is.True);
    }

    [TestCase<int>()]
    [TestCase<double>()]
    [TestCase<MyStruct>()]
    public void ExpandSuccessfully<T>() where T : unmanaged
    {
        using var list = new UnmanagedList<T>(8);

        for (var i = 0; i < 15; i++)
            list.Add(default);

        Assert.That(list.Capacity, Is.GreaterThan(8));
    }

    [Test]
    public void GetSpanSuccessfully()
    {
        using var list = new UnmanagedList<MyStruct>(8);

        var count = 16;

        for (var i = 0; i < count; i++)
            list.Add(new MyStruct()
            {
                A = i,
                B = i * 2
            });

        var span = list.Span(0, list.Count);

        for (var i = 0; i < count; i++)
        {
            Assert.That(span[i].A, Is.EqualTo(i));
            Assert.That(span[i].B, Is.EqualTo(i * 2));
        }
    }

    [Test]
    public void GetSpanThrows()
    {
        using var list = new UnmanagedList<int>(8);

        list.Add(default);

        Assert.Throws<IndexOutOfRangeException>(() => { list.Span(0, 2); });
    }

    [Test]
    public void GetSpanThrowsNegativeIndex()
    {
        using var list = new UnmanagedList<int>(8);

        list.Add(default);

        Assert.Throws<IndexOutOfRangeException>(() => { list.Span(-1, 1); });
    }

    [Test]
    public void GetSpanThrowsNegativeCount()
    {
        using var list = new UnmanagedList<int>(8);

        list.Add(default);

        Assert.Throws<IndexOutOfRangeException>(() => { list.Span(0, -1); });
    }

    [Test]
    public void TestCastSpan()
    {
        using var list = new UnmanagedList<byte>(8);
        list.Add(200);

        var casted = list.Casted<sbyte>(0, 1);

        Assert.That(casted[0], Is.EqualTo(unchecked((sbyte) 200)));
    }

    private static void MyMethod(int d, object obj)
    {
        Console.WriteLine($"Param is {d} {obj}");
    }

    [Test]
    public void FuncPointers()
    {
        var method =
            typeof(UnmanagedListTests).GetMethod(nameof(MyMethod), BindingFlags.Static | BindingFlags.NonPublic);

        var caller = new VoidCaller<int>(method);

        using var stack = new Stack();

        var stringVar = "asd";
        var intValue = 123;
        stack.Push(ref intValue);
        stack.PushReference(stringVar);

        intValue = 2;

        caller.Call(stack);
    }

    public class VoidCaller<A>
    {
        private delegate*<A, void> _func;

        public VoidCaller(MethodInfo method)
        {
            var parameters = method.GetParameters();

            if (parameters.Length != 2)
                throw new Exception("Invalid parameter count");

            if (parameters[0].ParameterType != typeof(A))
                throw new Exception();

            _func = (delegate*<A, void>) method.MethodHandle.GetFunctionPointer();
        }

        public void Call(Stack value)
        {
            var a = value.Pop<A>();
            //_func(a, b);
        }
    }

    public class Stack : IDisposable
    {
        private IntPtr* _stack;

        public const int StackSize = 128;

        public Stack()
        {
            _stack = (IntPtr*) AlignedAllocator.AllocateAligned(StackSize * sizeof(IntPtr), sizeof(IntPtr));
        }

        public void Dispose()
        {
            AlignedAllocator.Release((IntPtr) _stack);
            _stack = null;
        }

        private int _index;

        public void Push<T>(ref T t) where T : unmanaged
        {
            if (sizeof(T) > sizeof(IntPtr))
                throw new Exception("Invalid size");

            Unsafe.Copy(&_stack[_index], ref t);
            _index++;
        }

        public void PushReference(object t)
        {
            Unsafe.Copy(&_stack[_index], ref t);
            _index++;
        }

        public T Pop<T>()
        {
            if (Unsafe.SizeOf<T>() > sizeof(IntPtr))
                throw new Exception("Invalid size");

            _index--;
            var value = Unsafe.AsRef<T>(&_stack[_index]);
            return value;
        }
    }
}