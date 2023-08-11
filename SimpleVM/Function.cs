using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleVM;

public class FunctionArgument
{
    public Type Type { get; }
    public string Name { get; }

    public BuiltInType BuiltInType => BuiltInTypeUtils.GetBuiltInType(Type);

    public int WordSize => Word.GetWordSize(BuiltInType);

    public FunctionArgument(string name, Type type)
    {
        Type = type;
        Name = name;
    }
}

public class FunctionDefinition
{
    public string Name { get; }

    public Type ReturnType { get; }

    public FunctionArgument[] Arguments { get; }

    public int ArgumentsWordCount
    {
        get
        {
            var wordCount = 0;
            foreach (var argument in Arguments)
                wordCount += argument.WordSize;
            return wordCount;
        }
    }

    public int ReturnWordCount
    {
        get
        {
            if (ReturnType == typeof(void))
                return 0;

            return Word.GetWordSize(BuiltInTypeUtils.GetBuiltInType(ReturnType));
        }
    }

    public int GetWordIndexFor(FunctionArgument argument)
    {
        //todo improve this
        var index = 0;
        for (var i = 0; i < Arguments.Length; i++)
        {
            if (Arguments[i] == argument)
                return index;
            index += Arguments[i].WordSize;
        }

        throw new Exception();
    }

    public FunctionDefinition(string name, Type returnType, FunctionArgument[] arguments)
    {
        Name = name;
        ReturnType = returnType;
        Arguments = arguments;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(ReturnType.Name);
        builder.Append(" ");
        builder.Append(Name);
        builder.Append("(");
        var length = Arguments.Length;
        for (var i = 0; i < length; i++)
        {
            var argument = Arguments[i];
            builder.Append(argument.Type.Name);
            builder.Append(" ");
            builder.Append(argument.Name);
            if (i < length - 1)
                builder.Append(", ");
        }

        builder.Append(")");
        return builder.ToString();
    }
}

public class Function : IDisposable
{
    public Chunk Chunk;

    public readonly int ArgumentsWordCount;

    public readonly int ReturnWordCount;

    public readonly FunctionDefinition Definition;

    public Function(Chunk chunk, FunctionDefinition definition)
    {
        Chunk = chunk;

        ArgumentsWordCount = definition.ArgumentsWordCount;
        ReturnWordCount = definition.ReturnWordCount;
        Definition = definition;
    }

    public void Dispose()
    {
        Chunk.Dispose();
        Chunk = default;
    }

    public unsafe void Invoke()
    {
        var returnWords = ReturnWordCount;
        if (returnWords != 0)
            throw new VMRuntimeException("Function does not return a value");

        if (Definition.Arguments.Length != 0)
            throw new VMRuntimeException("Argument type does not match function argument type");

        var vmThreadData = VMThread.Create();
        var stack = Stack.Create();

        try
        {
            var offset = stack.StackOffset;

            if (ArgumentsWordCount != (int)offset)
                throw new Exception("Incorrect number of arguments on the stack");

            VMRunner.DoCall(ref vmThreadData, ref stack, this);

            VMRunner.Run(ref vmThreadData, ref stack);

            if (stack.StackOffset != ReturnWordCount)
            {
                throw new VMRuntimeException(
                    $"Incorrect values on the stack. Expected {ReturnWordCount} got {stack.StackOffset}");
            }
        }
        finally
        {
            vmThreadData.Dispose();
            stack.Dispose();
        }
    }

    public unsafe Func<T, T1, TReturn> GetAction<T, T1, TReturn>()
    {
        var returnWords = ReturnWordCount;

        if (returnWords == 0)
            throw new VMRuntimeException("Function does not return a value");

        if (Definition.ReturnType != typeof(TReturn))
            throw new VMRuntimeException("Return type does not match function return type");

        if (Definition.Arguments[0].Type != typeof(T))
            throw new VMRuntimeException("Argument type does not match function argument type");

        if (Definition.Arguments[1].Type != typeof(T1))
            throw new VMRuntimeException("Argument type does not match function argument type");

        return (a, b) =>
        {
            var vmThreadData = VMThread.Create();
            var stack = Stack.Create();

            try
            {
                //i don't think we need a normal handle since we have the object on the stack
                stack.SafePush(a);
                stack.SafePush(b);

#if DEBUG
                var offset = stack.StackOffset;
                if (ArgumentsWordCount != (int)offset)
                    throw new Exception("Incorrect number of arguments on the stack");
#endif
                
                VMRunner.DoCall(ref vmThreadData, ref stack, this);
                VMRunner.Run(ref vmThreadData, ref stack);

#if DEBUG
                if (stack.StackOffset != ReturnWordCount)
                {
                    throw new VMRuntimeException(
                        $"Incorrect values on the stack. Expected {ReturnWordCount} got {stack.StackOffset}");
                }
#endif
                
                return stack.SafePop<TReturn>();
            }
            finally
            {
                vmThreadData.Dispose();
                stack.Dispose();
            }
        };
    }
}