using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public interface INode
    {
        void Print(ReadOnlySpan<char> asSpan, StringBuilder builder);

        void Prepare(ReadOnlySpan<char> asSpan)
        {
            
        }
    }
}