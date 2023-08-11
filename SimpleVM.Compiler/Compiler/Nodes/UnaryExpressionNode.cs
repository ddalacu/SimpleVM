using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class UnaryExpressionNode : INode
    {
        public TokenData Operator { get; }
        public INode Operand { get; set; }

        public UnaryExpressionNode(TokenData @operator, INode operand)
        {
            Operator = @operator;
            Operand = operand;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            builder.Append('(');
            builder.Append(Operator.GetText(asSpan));
            Operand.Print(asSpan, builder);
            builder.Append(')');
        }
    }
}