using System;

namespace SimpleVM.Compiler
{
    public struct TokenizerEntry
    {
        public Token Token;
        public string Str;
    }

    public ref struct Tokenizer
    {
        private readonly ReadOnlySpan<char> _span;

        private int _charIndex;

        public Tokenizer(ReadOnlySpan<char> span)
        {
            _span = span;
            _charIndex = 0;
        }

        public bool AdvanceToken(ref TokenData data)
        {
            data = new TokenData();

            if (_charIndex > _span.Length)
                return false;

            data.Start = _charIndex;

            if (_charIndex == _span.Length)
            {
                data.Token = Token.EOF;
                _charIndex++;
                return true;
            }

            try
            {
                if (CheckWhitespace())
                {
                    data.Token = Token.WhiteSpace;
                    return true;
                }

                if (CheckInlineComment())
                {
                    data.Token = Token.Comment;
                    return true;
                }

                if (CheckComment())
                {
                    data.Token = Token.Comment;
                    return true;
                }

                if (CheckSpecial(ref data.Token))
                    return true;

                if (CheckNumber())
                {
                    data.Token = Token.NumericLiteral;
                    return true;
                }

                if (CheckIdentifier())
                {
                    var length = _charIndex - data.Start;
                    var identifierSpan = _span.Slice(data.Start, length);

                    if (TokenizerHelper.MatchKeyword(identifierSpan))
                    {
                        data.Token = Token.Keyword;
                    }
                    else
                    {
                        data.Token = Token.Identifier;
                    }

                    return true;
                }

                data.Token = Token.Unknown;
                _charIndex++;
                return true;
            }
            finally
            {
                data.Length = _charIndex - data.Start;
            }
        }

        private bool TryAdvanceChar(out char ch)
        {
            _charIndex++;
            if (_charIndex < _span.Length)
            {
                ch = _span[_charIndex];
                return true;
            }

            ch = default;
            return false;
        }

        private bool CheckWhitespace()
        {
            var currentChar = _span[_charIndex];

            if (char.IsWhiteSpace(currentChar) == false &&
                currentChar != '\t')
                return false;

            do
            {
                if (TryAdvanceChar(out currentChar) == false)
                    return true;
            } while (char.IsWhiteSpace(currentChar) ||
                     currentChar == '\t');

            return true;
        }

        private bool TryPeekNext(int count, out ReadOnlySpan<char> span)
        {
            if (_charIndex + count < _span.Length)
            {
                span = _span.Slice(_charIndex, count);
                return true;
            }

            span = default;
            return false;
        }

        private bool CheckInlineComment()
        {
            if (TryPeekNext(2, out var span) == false)
                return false;

            if (span.SequenceEqual("//".AsSpan()) == false)
                return false;

            _charIndex += 2;

            var currentChar = _span[_charIndex];

            while (currentChar != '\r' && currentChar != '\n')
            {
                if (TryAdvanceChar(out currentChar) == false)
                    return true;
            }

            return true;
        }

        private bool CheckComment()
        {
            if (TryPeekNext(2, out var span) == false)
                return false;

            if (span.SequenceEqual("/*".AsSpan()) == false)
                return false;

            _charIndex += 2;

            while (TryPeekNext(2, out span))
            {
                if (span.SequenceEqual("*/".AsSpan()))
                {
                    _charIndex += 2;
                    return true;
                }

                if (TryAdvanceChar(out _) == false)
                    return true;
            }

            return true;
        }

        private bool CheckCharEntry(TokenizerEntry entry, ref Token token)
        {
            var entrySpan = entry.Str.AsSpan();

            if (_charIndex + entrySpan.Length > _span.Length)
                return false;

            var span = _span.Slice(_charIndex, entrySpan.Length);

            if (span.SequenceEqual(entrySpan) == false)
                return false;

            _charIndex += entrySpan.Length;
            token = entry.Token;
            return true;
        }

        private bool CheckSpecial(ref Token token)
        {
            foreach (var entry in
                     TokenizerHelper
                         .Entries) //todo i think this can be optimizer by using proper algorithm, but for now it's ok
            {
                if (CheckCharEntry(entry, ref token))
                    return true;
            }

            return false;
        }

        private bool CheckNumber()
        {
            var currentChar = _span[_charIndex];

            if (char.IsDigit(currentChar) == false &&
                currentChar != '.') return false;

            var haveDecimalPoint = false;
            while (char.IsDigit(currentChar) || (!haveDecimalPoint && currentChar == '.'))
            {
                haveDecimalPoint = currentChar == '.';

                if (TryAdvanceChar(out currentChar) == false)
                    break;
            }

            return true;
        }

        private bool CheckIdentifier()
        {
            var currentChar = _span[_charIndex];

            if (char.IsLetter(currentChar) == false && currentChar != '_')
                return false;

            do
            {
                if (TryAdvanceChar(out currentChar) == false)
                    break;
            } while (char.IsLetter(currentChar) ||
                     currentChar == '_' ||
                     char.IsNumber(currentChar));

            return true;
        }
    }
}