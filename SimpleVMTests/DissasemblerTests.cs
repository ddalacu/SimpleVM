using System.Diagnostics;
using System.Runtime.InteropServices;
using SimpleVM;
using SimpleVM.Collections;

namespace SimpleVMTests;

public unsafe class DissasemblerTests
{
    public class SumBuilder<T>
    {
        private static FunctionDefinition SumFunctionDefinition = new FunctionDefinition("Sum", typeof(T),
            new FunctionArgument[]
            {
                new FunctionArgument("A", typeof(T)),
                new FunctionArgument("B", typeof(T)),
            });

        public static Function CreateSumFunction(LineTracker lineTracker)
        {
            using var codeBlockWriter = new CodeBlockWriter(8);
            using var constants = new ConstantsBufferWriter(8);

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_GET_LOCAL,
                GetLocalData = new GetLocalData()
                {
                    Type = SumFunctionDefinition.Arguments[0].BuiltInType, //load arg 0
                    Offset = (byte)SumFunctionDefinition.GetWordIndexFor(SumFunctionDefinition.Arguments[0])
                }
            }, codeBlockWriter, lineTracker, LineNumber.Zero); //load arg 0

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_GET_LOCAL,
                GetLocalData = new GetLocalData()
                {
                    Type = SumFunctionDefinition.Arguments[1].BuiltInType, //load arg 0
                    Offset = (byte)SumFunctionDefinition.GetWordIndexFor(SumFunctionDefinition.Arguments[1])
                }
            }, codeBlockWriter, lineTracker, LineNumber.Zero); //load arg 0

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_ADD,
                AddData = new AddData()
                {
                    Type = BuiltInTypeUtils.GetBuiltInType(SumFunctionDefinition.ReturnType)
                }
            }, codeBlockWriter, lineTracker, LineNumber.Zero);

            // var sumCaller = new FunctionCaller<string, string, string>((a, b) => a + b);
            //
            // OpCodeUtils.Write(new OPData()
            // {
            //     Code = OpCode.OP_EXTERNAL_CALL,
            //     ExternalCallData = new ExternalCallData()
            //     {
            //         Position = constants.Write(sumCaller)
            //     }
            // }, codeBlockWriter, lineTracker, LineNumber.Zero);

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_RETURN
            }, codeBlockWriter, lineTracker, LineNumber.Zero);

            var chunk = new Chunk()
            {
                CodeBlock = codeBlockWriter.CreateCode(),
                Constants = constants.CreateBuffer(),
                Tracker = lineTracker
            };

            return new Function(chunk, SumFunctionDefinition);
        }


        public static Function CreateOperationFunction(LineTracker lineTracker, Func<T, T, T> action)
        {
            using var codeBlockWriter = new CodeBlockWriter(8);
            using var constants = new ConstantsBufferWriter(8);

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_GET_LOCAL,
                GetLocalData = new GetLocalData()
                {
                    Type = SumFunctionDefinition.Arguments[0].BuiltInType, //load arg 0
                    Offset = (byte)SumFunctionDefinition.GetWordIndexFor(SumFunctionDefinition.Arguments[0])
                }
            }, codeBlockWriter, lineTracker, LineNumber.Zero); //load arg 0

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_GET_LOCAL,
                GetLocalData = new GetLocalData()
                {
                    Type = SumFunctionDefinition.Arguments[1].BuiltInType, //load arg 0
                    Offset = (byte)SumFunctionDefinition.GetWordIndexFor(SumFunctionDefinition.Arguments[1])
                }
            }, codeBlockWriter, lineTracker, LineNumber.Zero); //load arg 0

            var sumCaller = new FunctionCaller<T, T, T>(action);

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_EXTERNAL_CALL,
                ExternalCallData = new ExternalCallData()
                {
                    Position = constants.Write(sumCaller)
                }
            }, codeBlockWriter, lineTracker, LineNumber.Zero);

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_RETURN
            }, codeBlockWriter, lineTracker, LineNumber.Zero);

            var chunk = new Chunk()
            {
                CodeBlock = codeBlockWriter.CreateCode(),
                Constants = constants.CreateBuffer(),
                Tracker = lineTracker
            };

            return new Function(chunk, SumFunctionDefinition);
        }


        public static void WriteSumCall(CodeBlockWriter codeBlockWriter, ConstantsBufferWriter constants,
            LineTracker lineTracker,
            Function function, T a, T b)
        {
            OpCodeUtils.WriteConst(codeBlockWriter, constants, lineTracker, LineNumber.Zero,
                a); //push argument with value 1
            OpCodeUtils.WriteConst(codeBlockWriter, constants, lineTracker, LineNumber.Zero,
                b); //push argument with value 2

            OpCodeUtils.WriteCall(codeBlockWriter, constants, lineTracker, LineNumber.Zero, function); //call method

            OpCodeUtils.Write(new OPData()
            {
                Code = OpCode.OP_POP,
                PopData = new PopData()
                {
                    Type = BuiltInTypeUtils.GetBuiltInType<T>()
                }
            }, codeBlockWriter, lineTracker, LineNumber.Zero); //discard result
        }
    }

    [Test]
    public void TestOperation()
    {
        using var lineTracker = new LineTracker();

        var function = SumBuilder<string>.CreateOperationFunction(lineTracker, string.Concat);
        var action = function.GetAction<string, string, string>();

        var result = action("a", "b");

        function.Dispose();
        Assert.That(result, Is.EqualTo("ab"));
    }


    [Test]
    public void TestVMReturns()
    {
        using var lineTracker = new LineTracker();

        var function = SumBuilder<int>.CreateSumFunction(lineTracker);

        var action = function.GetAction<int, int, int>();

        var result = action(1, 2);
        Console.WriteLine(result);

        var watch = Stopwatch.StartNew();
        for (var i = 0; i < 1_000_000; i++)
            action(1, 2);

        watch.Stop();
        Console.WriteLine(watch.Elapsed.TotalMilliseconds);

        function.Dispose();
        Assert.That(result, Is.EqualTo(3));
    }

    [Test]
    public void TestVM()
    {
        using var lineTracker = new LineTracker();

        var function = SumBuilder<uint>.CreateSumFunction(lineTracker);

        using var codeBlockWriter = new CodeBlockWriter(8);

        using var constants = new ConstantsBufferWriter(8);

        SumBuilder<uint>.WriteSumCall(codeBlockWriter, constants, lineTracker, function, 1, 2);

        OpCodeUtils.Write(new OPData()
        {
            Code = OpCode.OP_RETURN
        }, codeBlockWriter, lineTracker, LineNumber.Zero);

        var emptyFunctionDefinition = new FunctionDefinition("Caller", typeof(void), Array.Empty<FunctionArgument>());

        var callerFunction = new Function(new Chunk()
        {
            CodeBlock = codeBlockWriter.CreateCode(),
            Constants = constants.CreateBuffer(),
            Tracker = lineTracker
        }, emptyFunctionDefinition);

        callerFunction.Invoke();

        callerFunction.Dispose();
    }
}