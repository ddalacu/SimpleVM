using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleVM.Compiler
{
    public class InvocationExpressionNode : INode
    {
        public INode IdentifierName { get; }
        public List<INode> Arguments { get; }

        public InvocationExpressionNode(INode node, List<INode> arguments)
        {
            IdentifierName = node;
            Arguments = arguments;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            IdentifierName.Print(asSpan, builder);
            builder.Append('(');

            for (var i = 0; i < Arguments.Count; i++)
            {
                Arguments[i].Print(asSpan, builder);
                if (i != Arguments.Count - 1)
                {
                    builder.Append(", ");
                }
            }

            builder.Append(')');
        }
    }
}