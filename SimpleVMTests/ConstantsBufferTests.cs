using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SimpleVM.Collections;

namespace SimpleVMTests;

public class ConstantsBufferTests
{
    [TestCase<int>()]
    [TestCase<double>()]
    [TestCase<float>()]
    public void WriteAndReadCorrectly<T>() where T : unmanaged
    {
        using var writer = new ConstantsBufferWriter(8);

        var value = DataUtils.CreateRandom<T>();

        var count = 512;
        var array = new T[count];
        var positions = new ConstantPosition[count];

        for (var i = 0; i < count; i++)
        {
            writer.Write<byte>(123);
            positions[i] = writer.Write(value);
            writer.Write<byte>(123);
            array[i] = value;
        }

        using var constants = writer.CreateBuffer();

        for (var i = 0; i < count; i++)
        {
            var position = positions[i];
            var expected = array[i];

            var actual = constants.Read<T>(position);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    private static void MyMethod(ConstantsBufferWriter writer, out WeakReference weakReference)
    {
        var instance = new List<int>();
        writer.Write(instance);
        weakReference = new WeakReference(instance);
    }

    [Test]
    public void ConstantsKeepObjectsAlive()
    {
        using var writer = new ConstantsBufferWriter(32);
        MyMethod(writer, out var weakReference);

        var constants = writer.CreateBuffer();

        GC.Collect();
        Assert.That(weakReference.IsAlive, Is.True);

        constants.Dispose();

        GC.Collect();
        GC.WaitForFullGCComplete();
        Assert.That(weakReference.IsAlive, Is.False);
    }

    private static unsafe void GetWeakRef(ref IntPtr* stack, out WeakReference weakReference)
    {
        var instance = new List<int>();
        weakReference = new WeakReference(instance);
        Unsafe.Write(stack, instance);
        stack++;
        
        GC.KeepAlive(instance);
    }
    
    
    [Test]
    public unsafe void GarbageCollectedTest()
    {
        var count = 4;

        var start = stackalloc IntPtr[count];
        var stack = start;

        var weakRefs = new WeakReference[count];

        for (var i = 0; i < count; i++)
        {
            GetWeakRef(ref stack, out var weakRef);
            weakRefs[i] = weakRef;
            
        }

        for (var i = 0; i < count; i++)
        {
            Console.WriteLine($"{i} alive {weakRefs[i].IsAlive}");
            Assert.That(weakRefs[i].IsAlive, Is.True);
        }

        GC.Collect();
        GC.WaitForFullGCComplete();
        
        for (var i = 0; i < count; i++)
        {
            Console.WriteLine($"{i} alive {weakRefs[i].IsAlive}");
            Assert.That(weakRefs[i].IsAlive == false, Is.True);
        }
    }
}