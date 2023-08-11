using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SimpleVM;

[StructLayout(LayoutKind.Auto)]
public unsafe struct CallFrame
{
    public Function Function;

    public OPData* IP;
    
    public Word* StackStart;
    
    public CallFrame(Function function, Word* start)
    {
        Function = function;
        StackStart = start;
        IP = Function.Chunk.CodeBlock.Pointer;
    }

    public int GetCodeOffset() => (int) (IP - Function.Chunk.CodeBlock.Pointer);

    public OPData ReadOpCodeData() => (*IP++);
}