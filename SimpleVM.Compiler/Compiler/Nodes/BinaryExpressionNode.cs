using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class BinaryExpressionNode : INode, IProvideValue
    {
        public INode Left { get; set; }
        public TokenData C { get; }

        public INode Right { get; set; }

        public BinaryExpressionNode(INode left, TokenData tokenData, INode right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));
            if (right == null)
                throw new ArgumentNullException(nameof(right));

            Left = left;
            C = tokenData;
            Right = right;
        }

        public override string ToString()
        {
            return $"({Left} {C} {Right})";
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            builder.Append('(');
            Left.Print(asSpan, builder);
            builder.Append(C.GetText(asSpan));
            Right.Print(asSpan, builder);
            builder.Append(')');
        }

        public void Prepare(ReadOnlySpan<char> span)
        {
            if (Left == null)
                throw new Exception("Left is null");
            if (Right == null)
                throw new Exception("Right is null");

            if (Left is IProvideValue leftProvider == false)
                return;

            if (Right is IProvideValue rightProvider == false)
                return;

            Left.Prepare(span);
            Right.Prepare(span);

            var leftType = leftProvider.GetValueType();
            var rightType = rightProvider.GetValueType();

            if (leftType != rightType)
                throw new Exception("Types are not same");
        }

        public Type GetValueType()
        {
            var leftProvider = Left as IProvideValue;
            var rightProvider = Right as IProvideValue;

            var leftType = leftProvider.GetValueType();
            var rightType = rightProvider.GetValueType();

            return leftType;
        }
    }
    
    public interface IProvideValue
    {
        public Type GetValueType();
    }
}