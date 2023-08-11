using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SimpleVM.Collections;

namespace SimpleVM;

public static unsafe class VMRunner
{
    internal static void Run(ref VMThread vmThread, ref Stack stack)
    {
        start:

        ref var frame = ref vmThread.CurrentFrame;
        var codeStruct = frame.ReadOpCodeData();

        switch (codeStruct.Code)
        {
            case OpCode.OP_CONSTANT:
            {
                LoadConstant(ref stack, ref frame.Function.Chunk.Constants, codeStruct.ConstantData.Type,
                    codeStruct.ConstantData.Position);
                break;
            }
            case OpCode.OP_POP:
            {
                OpPopDiscard(ref stack, codeStruct.PopData.Type);
                break;
            }
            case OpCode.OP_ADD:
            {
                DoAddition(ref stack, codeStruct.AddData.Type);
#if DEBUG
                Console.WriteLine($"Addition {codeStruct.AddData.Type}");
#endif
                break;
            }
            case OpCode.OP_EXTERNAL_CALL:
            {
                var function = frame.Function.Chunk.Constants.ReadObj<ICall>(codeStruct.ExternalCallData.Position);
#if DEBUG
                Console.WriteLine($"Calling managed function {function}");
#endif
                function.Call(ref stack);
                break;
            }
            case OpCode.OP_CALL:
            {
                var function = frame.Function.Chunk.Constants.ReadObj<Function>(codeStruct.CallData.Position);
                DoCall(ref vmThread, ref stack, function);
                break;
            }
            case OpCode.OP_RETURN:
            {
                if (DoReturn(ref vmThread, ref stack, frame) == false)
                    return;
                break;
            }
            case OpCode.OP_SET_LOCAL:
            {
                var offset = codeStruct.SetLocalData.Offset;
                OpSetLocal(ref stack, codeStruct.SetLocalData.Type, frame.StackStart + offset);
#if DEBUG
                Console.WriteLine($"Set local at index {offset}");
#endif
                break;
            }

            case OpCode.OP_GET_LOCAL:
            {
                var offset = codeStruct.GetLocalData.Offset;
                var builtInType = codeStruct.GetLocalData.Type;
                OpGetLocal(ref stack, builtInType,
                    frame.StackStart + offset);
#if DEBUG
                string isParam = offset < frame.Function.ArgumentsWordCount ? "arg" : "local";
                Console.WriteLine($"Push {isParam} Index={offset} DataSize={builtInType}");
#endif
                break;
            }
            default:
            {
                throw new VMRuntimeException($"Unknown opcode {codeStruct}");
            }
        }

        goto start;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OpPopDiscard(ref Stack vmThreadStack, BuiltInType builtInType)
    {
#if DEBUG
        Console.WriteLine($"Pop value of size {builtInType}");
#endif

        switch (builtInType)
        {
            case BuiltInType.Bool:
                vmThreadStack.PopDiscard<bool>();
                break;
            case BuiltInType.Byte:
                vmThreadStack.PopDiscard<byte>();
                break;
            case BuiltInType.SByte:
                vmThreadStack.PopDiscard<sbyte>();
                break;
            case BuiltInType.Char:
                vmThreadStack.PopDiscard<char>();
                break;
            case BuiltInType.Decimal:
                vmThreadStack.PopDiscard<decimal>();
                break;
            case BuiltInType.Double:
                vmThreadStack.PopDiscard<double>();
                break;
            case BuiltInType.Float:
                vmThreadStack.PopDiscard<float>();
                break;
            case BuiltInType.Int:
                vmThreadStack.PopDiscard<int>();
                break;
            case BuiltInType.UInt:
                vmThreadStack.PopDiscard<uint>();
                break;
            case BuiltInType.Long:
                vmThreadStack.PopDiscard<long>();
                break;
            case BuiltInType.ULong:
                vmThreadStack.PopDiscard<ulong>();
                break;
            case BuiltInType.Short:
                vmThreadStack.PopDiscard<short>();
                break;
            case BuiltInType.UShort:
                vmThreadStack.PopDiscard<ushort>();
                break;
            case BuiltInType.Object:
            {
                var handle = vmThreadStack.Pop<GCHandle>();
                if (handle != default)
                    handle.Free();
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(builtInType), builtInType, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OpGetLocal(ref Stack vmThreadStack, BuiltInType builtInType, Word* position)
    {
        switch (builtInType)
        {
            case BuiltInType.Bool:
                vmThreadStack.PushFrom<bool>(position);
                break;
            case BuiltInType.Byte:
                vmThreadStack.PushFrom<byte>(position);
                break;
            case BuiltInType.SByte:
                vmThreadStack.PushFrom<sbyte>(position);
                break;
            case BuiltInType.Char:
                vmThreadStack.PushFrom<char>(position);
                break;
            case BuiltInType.Decimal:
                vmThreadStack.PushFrom<decimal>(position);
                break;
            case BuiltInType.Double:
                vmThreadStack.PushFrom<double>(position);
                break;
            case BuiltInType.Float:
                vmThreadStack.PushFrom<float>(position);
                break;
            case BuiltInType.Int:
                vmThreadStack.PushFrom<int>(position);
                break;
            case BuiltInType.UInt:
                vmThreadStack.PushFrom<uint>(position);
                break;
            case BuiltInType.Long:
                vmThreadStack.PushFrom<long>(position);
                break;
            case BuiltInType.ULong:
                vmThreadStack.PushFrom<ulong>(position);
                break;
            case BuiltInType.Short:
                vmThreadStack.PushFrom<short>(position);
                break;
            case BuiltInType.UShort:
                vmThreadStack.PushFrom<ushort>(position);
                break;
            case BuiltInType.Object:

                var get = vmThreadStack.ReadFrom<GCHandle>(position);
                if (get.IsAllocated)
                {
                    var target = get.Target;
                    var copy = GCHandle.Alloc(target);

#if DEBUG
                    Console.WriteLine($"Allocated copy handle for {target}");
#endif

                    vmThreadStack.Push(copy);
                }
                else
                {
                    vmThreadStack.Push(default(GCHandle));
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(builtInType), builtInType, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void OpSetLocal(ref Stack vmThreadStack, BuiltInType builtInType, Word* position)
    {
        switch (builtInType)
        {
            case BuiltInType.Bool:
                vmThreadStack.WriteAt(vmThreadStack.Pop<bool>(), position);
                break;
            case BuiltInType.Byte:
                vmThreadStack.WriteAt(vmThreadStack.Pop<byte>(), position);
                break;
            case BuiltInType.SByte:
                vmThreadStack.WriteAt(vmThreadStack.Pop<sbyte>(), position);
                break;
            case BuiltInType.Char:
                vmThreadStack.WriteAt(vmThreadStack.Pop<char>(), position);
                break;
            case BuiltInType.Decimal:
                vmThreadStack.WriteAt(vmThreadStack.Pop<decimal>(), position);
                break;
            case BuiltInType.Double:
                vmThreadStack.WriteAt(vmThreadStack.Pop<double>(), position);
                break;
            case BuiltInType.Float:
                vmThreadStack.WriteAt(vmThreadStack.Pop<float>(), position);
                break;
            case BuiltInType.Int:
                vmThreadStack.WriteAt(vmThreadStack.Pop<int>(), position);
                break;
            case BuiltInType.UInt:
                vmThreadStack.WriteAt(vmThreadStack.Pop<uint>(), position);
                break;
            case BuiltInType.Long:
                vmThreadStack.WriteAt(vmThreadStack.Pop<long>(), position);
                break;
            case BuiltInType.ULong:
                vmThreadStack.WriteAt(vmThreadStack.Pop<ulong>(), position);
                break;
            case BuiltInType.Short:
                vmThreadStack.WriteAt(vmThreadStack.Pop<short>(), position);
                break;
            case BuiltInType.UShort:
                vmThreadStack.WriteAt(vmThreadStack.Pop<ushort>(), position);
                break;
            case BuiltInType.Object:
                vmThreadStack.WriteAt(vmThreadStack.Pop<GCHandle>(), position);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(builtInType), builtInType, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void LoadConstant(ref Stack stack, ref ConstantsBufferReader reader, BuiltInType builtInType,
        ConstantPosition constantPosition)
    {
        switch (builtInType)
        {
            case BuiltInType.Bool:
                stack.Push<bool>(reader.Read<bool>(constantPosition));
                break;
            case BuiltInType.Byte:
                stack.Push<byte>(reader.Read<byte>(constantPosition));
                break;
            case BuiltInType.SByte:
                stack.Push<sbyte>(reader.Read<sbyte>(constantPosition));
                break;
            case BuiltInType.Char:
                stack.Push<char>(reader.Read<char>(constantPosition));
                break;
            case BuiltInType.Decimal:
                stack.Push<decimal>(reader.Read<decimal>(constantPosition));
                break;
            case BuiltInType.Double:
                stack.Push<double>(reader.Read<double>(constantPosition));
                break;
            case BuiltInType.Float:
                stack.Push<float>(reader.Read<float>(constantPosition));
                break;
            case BuiltInType.Int:
                stack.Push<int>(reader.Read<int>(constantPosition));
                break;
            case BuiltInType.UInt:
                stack.Push<uint>(reader.Read<uint>(constantPosition));
                break;
            case BuiltInType.Long:
                stack.Push<long>(reader.Read<long>(constantPosition));
                break;
            case BuiltInType.ULong:
                stack.Push<ulong>(reader.Read<ulong>(constantPosition));
                break;
            case BuiltInType.Short:
                stack.Push<short>(reader.Read<short>(constantPosition));
                break;
            case BuiltInType.UShort:
                stack.Push<ushort>(reader.Read<ushort>(constantPosition));
                break;
            case BuiltInType.Object:
                stack.Push<GCHandle>(reader.Read<GCHandle>(constantPosition));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(builtInType), builtInType, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool DoReturn(ref VMThread vmThread, ref Stack stack, CallFrame frame)
    {
        var originalStack = stack.StackPointer;
        stack.StackPointer = frame.StackStart;

        var retWordCount = frame.Function.ReturnWordCount;

        if (retWordCount > 0)
        {
#if DEBUG
            var computedArgSize = (originalStack - frame.StackStart) - retWordCount;
            if (computedArgSize != frame.Function.ArgumentsWordCount)
            {
                throw new VMRuntimeException(
                    $"Incorrect number of arguments on the stack. Expected {frame.Function.ArgumentsWordCount} got {computedArgSize}");
            }
#endif
            //todo fix release handle
            //var wordIndex = 0;
            // foreach (var argument in frame.Function.Definition.Arguments)
            // {
            //     if (argument.Type.IsValueType == false)
            //     {
            //         var handle = vmThread.Stack.ReadFrom<GCHandle>(vmThread.Stack.StackPointer + wordIndex);
            //
            //         if (handle != default)
            //         {
            //             Console.WriteLine($"Return freeing handle {handle.Target}");
            //             handle.Free();
            //         }
            //     }
            //
            //     wordIndex += argument.WordSize;
            // }

            //frame.Function.Definition.Arguments[0].BuiltInType

            //move return values to the top of the stack
            for (var i = 0; i < retWordCount; i++)
            {
                stack.PushFromStack(originalStack - retWordCount + i);
            }
        }

#if DEBUG
        PrintFunctionReturn(ref vmThread,ref stack, frame.Function.Definition, retWordCount);
#endif

        return vmThread.PopFrame();
    }

#if DEBUG
    private static void PrintFunctionReturn(ref VMThread vmThread,ref Stack stack, FunctionDefinition definition, int retWordCount)
    {
        Console.Write($"Exit '{definition}'");

        if (definition.ReturnType != typeof(void))
        {
            Console.Write(" Return=");
            var boxedValue = stack.GetBoxed(stack.StackPointer - retWordCount,
                definition.ReturnType);

            Console.Write(boxedValue);
        }

        Console.WriteLine();
    }
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void DoCall(ref VMThread vmThread, ref Stack stack, Function function)
    {
        var returnPos = stack.StackPointer - function.ArgumentsWordCount;

#if DEBUG
        PrintFunctionCall(ref vmThread,ref stack, function.Definition, returnPos);
#endif

        if (vmThread.TryPushFrame(new CallFrame(function, returnPos)) == false)
        {
            throw new VMRuntimeException("Stack overflow.");
        }
    }

#if DEBUG
    [Conditional("DEBUG")]
    private static void PrintFunctionCall(ref VMThread vmThread,ref Stack stack, FunctionDefinition definition, Word* returnPos)
    {
        Console.Write($"Enter '{definition}'");

        var arguments = definition.Arguments;
        var argumentsLength = arguments.Length;
        if (argumentsLength > 0)
        {
            Console.Write(" Arguments=[");

            var pos = returnPos;
            for (var index = 0; index < argumentsLength; index++)
            {
                var argument = arguments[index];
                var argumentValue = stack.GetBoxed(pos, argument.Type);
                pos += argument.WordSize;
                Console.Write(argumentValue);
                if (index < argumentsLength - 1)
                    Console.Write(", ");
            }

            Console.Write("]");
        }

        Console.WriteLine();
    }
#endif

    public static void DoAddition(ref Stack stack, BuiltInType type)
    {
        switch (type)
        {
            case BuiltInType.Byte:
                var items = stack.PopTwo<byte>();
                stack.Push<byte>((byte)(items.Item1 + items.Item2));
                break;
            case BuiltInType.SByte:
                var items2 = stack.PopTwo<sbyte>();
                stack.Push<sbyte>((sbyte)(items2.Item1 + items2.Item2));
                break;
            case BuiltInType.Short:
                var items3 = stack.PopTwo<short>();
                stack.Push<short>((short)(items3.Item1 + items3.Item2));
                break;
            case BuiltInType.UShort:
                var items4 = stack.PopTwo<ushort>();
                stack.Push<ushort>((ushort)(items4.Item1 + items4.Item2));
                break;
            case BuiltInType.Int:
                var items5 = stack.PopTwo<int>();
                stack.Push<int>(items5.Item1 + items5.Item2);
                break;
            case BuiltInType.UInt:
                var items6 = stack.PopTwo<uint>();
                stack.Push<uint>(items6.Item1 + items6.Item2);
                break;
            case BuiltInType.Long:
                var items7 = stack.PopTwo<long>();
                stack.Push<long>(items7.Item1 + items7.Item2);
                break;
            case BuiltInType.ULong:
                var items8 = stack.PopTwo<ulong>();
                stack.Push<ulong>(items8.Item1 + items8.Item2);
                break;
            case BuiltInType.Float:
                var items9 = stack.PopTwo<float>();
                stack.Push<float>(items9.Item1 + items9.Item2);
                break;
            case BuiltInType.Double:
                var items10 = stack.PopTwo<double>();
                stack.Push<double>(items10.Item1 + items10.Item2);
                break;
            case BuiltInType.Decimal:
                var items11 = stack.PopTwo<decimal>();
                stack.Push<decimal>(items11.Item1 + items11.Item2);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }
}