using System.Text;
using Pidgin;
using SimpleVM.Compiler;
using Parser = SimpleVM.Compiler.Parser;

namespace SimpleVM.CompilerTests;

public class CompilerTests
{
    [Test]
    public void Compile()
    {
        var text = "1+2*3";

        var tokenDatas = new List<TokenData>();

        if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
        {
            Assert.Fail();
            return;
        }

        var result = Parser.ParseOrThrowExpression(tokenDatas);

        var builder = new StringBuilder();
        result.Print(text.AsSpan(), builder);

        result.Prepare(text.AsSpan());

        Console.WriteLine(builder.ToString());

        var valueProvider = result as IProvideValue;

        Console.WriteLine(valueProvider.GetValueType());
    }

    [Test]
    public void Statement()
    {
        var text = "int a,b; a = 1 + 2 * 3; b=3;";

        var tokenDatas = new List<TokenData>();

        if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
        {
            Assert.Fail();
            return;
        }

        //(1 + 3) = 1234;
        var result = Parser.Statements.ParseOrThrow(tokenDatas);

        var builder = new StringBuilder();

        foreach (var node in result)
        {
            node.Print(text.AsSpan(), builder);
            builder.AppendLine();

            node.Prepare(text.AsSpan());
        }

        Console.WriteLine(builder.ToString());

        // var valueProvider= result as IProvideValue;
        //
        // Console.WriteLine(valueProvider.GetValueType());
    }

    [Test]
    public void Declare()
    {
        var text = "int a,b,c;";

        var tokenDatas = new List<TokenData>();

        if (TokenizerHelper.GetTokens(text, tokenDatas) == false)
        {
            Assert.Fail();
            return;
        }

        var result = Parser.LocalDeclarationStatement.ParseOrThrow(tokenDatas);
        
        var builder = new StringBuilder();
        result.Print(text.AsSpan(), builder);
        Console.WriteLine(builder.ToString());
        
        // var result = ExprParser.Statements.ParseOrThrow(tokenDatas);
        //
        // var builder = new StringBuilder();
        //
        // foreach (var node in result)
        // {
        //     node.Print(text.AsSpan(), builder);
        //     builder.AppendLine();
        //     node.Prepare(text.AsSpan());
        // }
        //
        // Console.WriteLine(builder.ToString());
    }
}