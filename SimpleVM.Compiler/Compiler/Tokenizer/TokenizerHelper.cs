using System;
using System.Collections.Generic;

namespace SimpleVM.Compiler
{
    public static class TokenizerHelper
    {
        private static List<TokenizerEntry> _entries;

        public static List<TokenizerEntry> Entries
        {
            get
            {
                if (_entries == null)
                    _entries = LoadValues();

                return _entries;
            }
        }

        //todo maybe use a trie
        public static readonly List<string> Keywords = new List<string>
        {
            "bool",
            "byte",
            "char",
            "decimal",
            "double",
            "float",
            "int",
            "long",
            "object",
            "sbyte",
            "short",
            "string",
            "uint",
            "ulong",
            "ushort",
            "true",
            "false",
            "void",
        };

        public static bool MatchKeyword(ReadOnlySpan<char> span)
        {
            foreach (var keyword in Keywords)
                if (keyword == span.ToString())
                    return true;

            return false;
        }

        public static List<TokenizerEntry> LoadValues()
        {
            var entries = new List<TokenizerEntry>();

            foreach (var token in Enum.GetValues(typeof(Token)))
            {
                var tokenValue = (Token) token;

                var attributes = tokenValue.GetAttributeOfType<TokenValueAttribute>();

                foreach (var attribute in attributes)
                {
                    entries.Add(new TokenizerEntry
                    {
                        Token = tokenValue,
                        Str = attribute.Value
                    });
                }
            }

            return entries;
        }

        public static T[] GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (T[]) attributes;
        }

        public static bool GetTokens(string text, List<TokenData> tokenDatas)
        {
            var readOnlySpan = text.AsSpan();
            var tokenizer = new Tokenizer(readOnlySpan);

            TokenData data = default;
            
            while (tokenizer.AdvanceToken(ref data))
            {
                if (data.IsValid == false)
                    throw new Exception($"Invalid token {data.GetText(readOnlySpan)}");

                if (data.Token == Token.Comment)
                    continue;
                if (data.Token == Token.WhiteSpace)
                    continue;

                tokenDatas.Add(data);
            }

            if (data.Token != Token.EOF)
                return false;

            return true;
        }
    }
}