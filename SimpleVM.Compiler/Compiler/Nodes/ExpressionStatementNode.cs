using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class ExpressionStatementNode : INode
    {
        public INode Expression { get; set; }

        public ExpressionStatementNode(INode expression)
        {
            Expression = expression;
        }

        public override string ToString()
        {
            return Expression + ";";
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            Expression.Print(asSpan, builder);
            builder.Append(';');
        }
    }
}