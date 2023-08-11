using System;

namespace SimpleVM.Compiler
{
    public struct TokenData
    {
        public Token Token;
        
        public int Start;
        
        public int Length;
        
        public bool IsValid => Token != Token.Unknown;
        
        public string GetText(ReadOnlySpan<char> span)
        {
            return span.Slice(Start, Length).ToString();
        }
    }
}