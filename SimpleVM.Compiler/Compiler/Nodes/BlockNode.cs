using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleVM.Compiler
{
    public class BlockNode : INode
    {
        public List<INode> Statements { get; set; }

        public BlockNode(List<INode> statements)
        {
            Statements = statements;
        }

        public override string ToString()
        {
            if (Statements == null)
                return "{}";

            return "{ " + string.Join(" ", Statements) + " }";
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            if (Statements == null || Statements.Count == 0)
            {
                builder.Append("{}");
                return;
            }

            builder.Append("{ ");
            for (var i = 0; i < Statements.Count; i++)
            {
                Statements[i].Print(asSpan, builder);
                if (i != Statements.Count - 1)
                {
                    builder.Append(' ');
                }
            }

            builder.Append(" }");
        }
    }
}