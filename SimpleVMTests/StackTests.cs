using SimpleVM;

namespace SimpleVMTests;

public class StackTests
{
    [TestCase<byte>()]
    [TestCase<short>()]
    [TestCase<int>()]
    [TestCase<double>()]
    [TestCase<float>()]
    public unsafe void PushFromWorks<T>() where T : unmanaged
    {
        using var stack = Stack.Create();

        var initialStack = stack.StackPointer;
        var value = DataUtils.CreateRandom<T>();
        stack.Push(value);

        Assert.That(stack.StackOffset, Is.EqualTo(Word.GetRequiredWords<T>()));

        stack.PushFrom<T>(initialStack);
        
        var popped = stack.Pop<T>();
        Assert.That(popped, Is.EqualTo(value));
        
        Assert.That(stack.StackOffset, Is.EqualTo(Word.GetRequiredWords<T>()));
    }
}