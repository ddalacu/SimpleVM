using System;
using System.Runtime.InteropServices;
using SimpleVM.Collections;

namespace SimpleVM;

public static class OpCodeUtils
{
    public static void WriteConst<T>(CodeBlockWriter code, ConstantsBufferWriter constants, LineTracker lineTracker,
        LineNumber codeLine, T value)
    {
        var constantBufferPosition = constants.Write(value);

        lineTracker.AddLine(code.Position, codeLine);

        code.Add(new OPData()
        {
            Code = OpCode.OP_CONSTANT,
            ConstantData = new ConstantData()
            {
                Type = BuiltInTypeUtils.GetBuiltInType(typeof(T)),
                Position = constantBufferPosition
            }
        });
    }

    public static void WriteCall(CodeBlockWriter code, ConstantsBufferWriter constants, LineTracker lineTracker,
        LineNumber codeLine, Function function)
    {
        var constantPosition = constants.Write(function);

        lineTracker.AddLine(code.Position, codeLine);

        code.Add(new OPData()
        {
            Code = OpCode.OP_CALL,
            CallData = new CallData()
            {
                Position = constantPosition
            }
        });
    }

    public static void Write(OPData str, CodeBlockWriter code, LineTracker lineTracker, LineNumber codeLine)
    {
        lineTracker.AddLine(code.Position, codeLine);
        code.Add(str);
    }
}