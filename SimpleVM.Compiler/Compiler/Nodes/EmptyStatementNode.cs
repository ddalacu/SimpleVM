using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class EmptyStatementNode : INode
    {
        public override string ToString()
        {
            return ";";
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            builder.Append(';');
        }
    }
}