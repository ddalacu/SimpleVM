using System.Runtime.CompilerServices;
using SimpleVM;

namespace SimpleVMTests;

public unsafe class AllocatorTests
{
    public struct MyStruct
    {
        public float Float;
        public double Double;
    }
    
    [Test]
    public void TestAllocator()
    {
        Console.WriteLine(Unsafe.SizeOf<MyStruct>());
        
        int alignment = 2;

        var block = AlignedAllocator.AllocateAligned(2048, alignment);
        
        var mask = alignment - 1;
        var masked = block.ToInt64() & mask;

        var isAligned = (masked ^ mask) == mask;
        
        Assert.IsTrue(isAligned);
        
        AlignedAllocator.Release(block);
    }
}