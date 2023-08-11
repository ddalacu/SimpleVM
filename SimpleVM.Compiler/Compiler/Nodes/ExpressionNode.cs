using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class ExpressionNode : INode, IProvideValue
    {
        public INode Expression { get; set; }

        public ExpressionNode(INode expression)
        {
            Expression = expression;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            builder.Append('(');
            Expression.Print(asSpan, builder);
            builder.Append(')');
        }

        public void Prepare(ReadOnlySpan<char> asSpan)
        {
            Expression.Prepare(asSpan);
        }

        public Type GetValueType()
        {
            if (Expression is IProvideValue provideValue == false)
                return null;

            return provideValue.GetValueType();
        }
    }
}