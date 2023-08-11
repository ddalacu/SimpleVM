using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class IdentifierNode : INode
    {
        public TokenData TokenData { get; }

        public IdentifierNode(TokenData tokenData)
        {
            TokenData = tokenData;
            // Value = value;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            builder.Append(TokenData.GetText(asSpan));
        }
    }
}