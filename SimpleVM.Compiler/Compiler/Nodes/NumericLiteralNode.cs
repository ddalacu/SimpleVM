using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class NumericLiteralNode : INode, IProvideValue
    {
        public TokenData TokenData { get; }

        private object _value;

        public NumericLiteralNode(TokenData tokenData)
        {
            if (tokenData.Token != Token.NumericLiteral)
                throw new ArgumentException("TokenData must be of type NumericLiteral", nameof(tokenData));
            
            TokenData = tokenData;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            builder.Append(TokenData.GetText(asSpan));
        }

        public void Prepare(ReadOnlySpan<char> asSpan)
        {
            var valueSpan = asSpan.Slice(TokenData.Start, TokenData.Length);
            
            _value = int.Parse(valueSpan);
        }

        public Type GetValueType()
        {
            return _value.GetType();
        }
    }
}