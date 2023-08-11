using System;
using System.Collections.Generic;

namespace SimpleVM.Compiler
{
    public class Compiler
    {
        public void Compile(string text)
        {
            var tokenDatas = new List<TokenData>();

            if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
            {
                //todo print string position
                throw new Exception("Failed to tokenize");
            }
            
            
        }
    }
}