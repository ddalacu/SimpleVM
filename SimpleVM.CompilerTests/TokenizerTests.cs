using SimpleVM.Compiler;

namespace SimpleVM.CompilerTests;

public class TokenizerTests
{
    [Test]
    public void TokenizeStringSuccessfully()
    {
        string.Join("  ", TokenizerHelper.Keywords);
        
        var text = "; + - * / ( ) , { } = %";

        var tokenDatas = new List<TokenData>();
        
        if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
        {
            Assert.Fail();
            return;
        }

        Assert.That(tokenDatas.Count, Is.EqualTo(12));
        Assert.That(tokenDatas[0].Token, Is.EqualTo(Token.Semicolon));
        Assert.That(tokenDatas[1].Token, Is.EqualTo(Token.Plus));
        Assert.That(tokenDatas[2].Token, Is.EqualTo(Token.Minus));
        Assert.That(tokenDatas[3].Token, Is.EqualTo(Token.Multiply));
        Assert.That(tokenDatas[4].Token, Is.EqualTo(Token.Divide));
        Assert.That(tokenDatas[5].Token, Is.EqualTo(Token.OpenParan));
        Assert.That(tokenDatas[6].Token, Is.EqualTo(Token.CloseParan));
        Assert.That(tokenDatas[7].Token, Is.EqualTo(Token.Comma));
        Assert.That(tokenDatas[8].Token, Is.EqualTo(Token.OpenBracket));
        Assert.That(tokenDatas[9].Token, Is.EqualTo(Token.CloseBracket));
        Assert.That(tokenDatas[10].Token, Is.EqualTo(Token.Equal));
        Assert.That(tokenDatas[11].Token, Is.EqualTo(Token.Modulo));
    }
    
    [Test]
    public void EatComment()
    {
        var text = "a /*comment*/ b";

        var tokenDatas = new List<TokenData>();
        
        if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
        {
            Assert.Fail();
            return;
        }
        
        Assert.That(tokenDatas.Count, Is.EqualTo(2));
        Assert.That(tokenDatas[0].Token, Is.EqualTo(Token.Identifier));
        Assert.That(tokenDatas[1].Token, Is.EqualTo(Token.Identifier));
    }
    
    [Test]
    public void EatLineComment()
    {
        var text = "a //comment \n b";

        var tokenDatas = new List<TokenData>();
        
        if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
        {
            Assert.Fail();
            return;
        }
        
        Assert.That(tokenDatas.Count, Is.EqualTo(2));
        Assert.That(tokenDatas[0].Token, Is.EqualTo(Token.Identifier));
        Assert.That(tokenDatas[1].Token, Is.EqualTo(Token.Identifier));
    }
    
    [Test]
    public void NumericLiteral()
    {
        var text = "a 123 b";

        var tokenDatas = new List<TokenData>();
        
        if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
        {
            Assert.Fail();
            return;
        }
        
        Assert.That(tokenDatas.Count, Is.EqualTo(3));
        Assert.That(tokenDatas[0].Token, Is.EqualTo(Token.Identifier));
        Assert.That(tokenDatas[1].Token, Is.EqualTo(Token.NumericLiteral));
        Assert.That(tokenDatas[2].Token, Is.EqualTo(Token.Identifier));
    }
    
    [Test]
    public void Identifier()
    {
        var text = "abc cdefg123";

        var tokenDatas = new List<TokenData>();
        
        if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
        {
            Assert.Fail();
            return;
        }
        
        Assert.That(tokenDatas.Count, Is.EqualTo(2));
        Assert.That(tokenDatas[0].Token, Is.EqualTo(Token.Identifier));
        Assert.That(tokenDatas[1].Token, Is.EqualTo(Token.Identifier));
    }
}