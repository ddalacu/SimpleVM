using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class AssignmentExpression : INode
    {
        public INode Left { get; set; }

        public INode Right { get; set; }

        public AssignmentExpression(INode left, INode right)
        {
            Left = left;
            Right = right;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            Left.Print(asSpan, builder);
            builder.Append(" = ");
            Right.Print(asSpan, builder);
        }
    }
}