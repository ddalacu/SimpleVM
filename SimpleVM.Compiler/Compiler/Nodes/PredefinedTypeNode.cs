using System;
using System.Text;

namespace SimpleVM.Compiler
{
    public class PredefinedTypeNode : INode
    {
        public TokenData Data { get; set; }

        public PredefinedTypeNode(TokenData tokenData)
        {
            if(tokenData.Token != Token.Keyword)
                throw new Exception("Token is not keyword");
            Data=tokenData;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            builder.Append(Data.GetText(asSpan));
        }
    }
}