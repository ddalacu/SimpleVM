using System;
using System.Runtime.CompilerServices;
using System.Text;
using SimpleVM.Collections;

namespace SimpleVM;

public enum LineNumber
{
    Zero = 0,
}

public class Dissasembler
{
    public static void Dissasemble(CodeBlock chunk, ConstantsBufferReader constants, LineTracker lineTracker,
        string name)
    {
        Console.WriteLine(name);

        for (var offset = 0; offset < chunk.Length;)
        {
            offset = UnpackInstruction(chunk, constants, lineTracker, offset);
        }
    }

    public static void UnpackInstruction(ref VMThread vmThread)
    {
        var frame = vmThread.CurrentFrame;
        var chunk = frame.Function.Chunk;
        Console.Write(frame.Function.Definition.Name + ":");
        UnpackInstruction(chunk.CodeBlock, chunk.Constants, chunk.Tracker, frame.GetCodeOffset());
    }

    public static int UnpackInstruction(CodeBlock chunk, ConstantsBufferReader constants, LineTracker lineTracker,
        int offset)
    {
        var instruction = chunk[offset];

        var instructionName = instruction.ToString();

        switch (instruction.Code)
        {
            case OpCode.OP_RETURN:
                SimpleInstruction(instructionName, lineTracker, offset);
                break;
            case OpCode.OP_CONSTANT:
                //ConstantInstructionByte(instructionName, chunk, constants, lineTracker, offset);
                SimpleInstruction(instructionName, lineTracker, offset);
                break;
            case OpCode.OP_POP:
                SimpleInstruction(instructionName, lineTracker, offset);
                break;
            case OpCode.OP_ADD:
                SimpleInstruction(instructionName, lineTracker, offset);
                break;
            case OpCode.OP_CALL:
                SimpleInstruction(instructionName, lineTracker, offset);
                break;
            case OpCode.OP_SET_LOCAL:
                SimpleInstruction(instructionName, lineTracker, offset);
                break;
            case OpCode.OP_GET_LOCAL:
                SimpleInstruction(instructionName, lineTracker, offset);
                break;
            default:
                throw new Exception($"Unknown opcode {instruction}");
        }

        return offset + 1;
    }

    private static void ConstantInstructionByte(string name, CodeBlock codeBlock, ConstantsBufferReader constants,
        LineTracker lineTracker, int offset)
    {
        var position = codeBlock.ReadUnaligned<ConstantPosition>(offset + 1);
        
        var value = constants.Read<int>(position);

        Console.Write(
            $"{name} const index:{position} value:{value}\n");
    }

    private static void SimpleInstruction(string name, LineTracker lineTracker, int offset)
    {
        Console.Write($"{name}\n");
    }
}