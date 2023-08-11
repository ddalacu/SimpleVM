using System;

namespace SimpleVM.Compiler
{
    public enum Token
    {
        Unknown,
        EOF,
        Keyword,
        NumericLiteral,
        Identifier,
        [TokenValue("+")]
        Plus,
        [TokenValue("-")]
        Minus,
        [TokenValue("*")]
        Multiply,
        [TokenValue("/")]
        Divide,
        [TokenValue("%")]
        Modulo,
        [TokenValue("(")]
        OpenParan,
        [TokenValue(")")]
        CloseParan,
        [TokenValue(",")]
        Comma,
        [TokenValue(";")]
        Semicolon,
        [TokenValue("=")]
        Equal,
        [TokenValue("{")]
        OpenBracket,
        [TokenValue("}")]
        CloseBracket,
        Comment,
        WhiteSpace,
    }

    public class TokenValueAttribute : Attribute
    {
        public string Value { get; }

        public TokenValueAttribute(string value)
        {
            Value = value;
        }
    }
}