using System;
using System.Runtime.InteropServices;
using SimpleVM.Collections;

namespace SimpleVM;

[StructLayout(LayoutKind.Auto)]
public struct Chunk : IDisposable
{
    public CodeBlock CodeBlock;
    
    public ConstantsBufferReader Constants;

    public LineTracker Tracker;
    
    public void Dispose()
    {
        if (CodeBlock.IsValid)
            CodeBlock.Dispose();
        if (Constants.IsValid)
            Constants.Dispose();
    }
}