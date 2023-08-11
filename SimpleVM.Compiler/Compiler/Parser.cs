using System;
using System.Collections.Generic;
using System.Linq;
using Pidgin;
using Pidgin.Expression;
using static Pidgin.Parser;

namespace SimpleVM.Compiler
{
    //slightly modified from Pidgin examples
    public static class Parser
    {
        private static Parser<TokenData, TokenData> Tok(Token token)
            => Try(Parser<TokenData>.Token(c => c.Token == token));

        private static Parser<TokenData, TokenData> Tokens(params Token[] token)
            => Try(Parser<TokenData>.Token(c => token.Contains(c.Token)));

        private static Parser<TokenData, T> Parenthesised<T>(Parser<TokenData, T> parser)
            => parser.Between(Tok(Token.OpenParan), Tok(Token.CloseParan));

        private static Parser<TokenData, Func<INode, INode, INode>> Binary(Parser<TokenData, TokenData> op)
            => op.Select<Func<INode, INode, INode>>(type => (l, r) => new BinaryExpressionNode(l, type, r));

        private static Parser<TokenData, Func<INode, INode>> Unary(Parser<TokenData, TokenData> op)
            => op.Select<Func<INode, INode>>(type => o => new UnaryExpressionNode(type, o));

        private static readonly Parser<TokenData, Func<INode, INode, INode>> _simple
            = Binary(Tokens(Token.Plus, Token.Minus));

        private static readonly Parser<TokenData, Func<INode, INode, INode>> _complex
            = Binary(Tokens(Token.Multiply, Token.Divide, Token.Modulo));

        private static readonly Parser<TokenData, Func<INode, INode>> _unary
            = Unary(Tok(Token.Minus));

        private static readonly Parser<TokenData, INode> _identifier
            = Tok(Token.Identifier)
                .Select<INode>(name => new IdentifierNode(name))
                .Labelled("Identifier");

        private static readonly Parser<TokenData, INode> _literal
            = Tok(Token.NumericLiteral)
                .Select<INode>(value => new NumericLiteralNode(value))
                .Labelled("Numeric Literal");

        private static Parser<TokenData, Func<INode, INode>> Call(Parser<TokenData, INode> subExpr)
            => Parenthesised(subExpr.Separated(Tok(Token.Comma)))
                .Select<Func<INode, INode>>(args => method => new InvocationExpressionNode(method, args.ToList()))
                .Labelled("Function Call");

        private static readonly Parser<TokenData, INode> _expr = ExpressionParser.Build<TokenData, INode>(
            expr => (
                OneOf(
                    _identifier,
                    _literal,
                    Parenthesised(expr).Labelled("parenthesised expression")
                ),
                new[]
                {
                    Operator.PostfixChainable(Call(expr)),
                    Operator.Prefix(_unary),
                    Operator.InfixL(_complex),
                    Operator.InfixL(_simple)
                }
            )
        ).Select<INode>(node => new ExpressionNode(node)).Labelled("Expression");

        private static readonly Parser<TokenData, INode> _assignment
            = Map((left, right) => { return (INode) new AssignmentExpression(left, right); }, _identifier
                .Before(Tok(Token.Equal)), _expr).Labelled("Assignment expression");

        public static readonly Parser<TokenData, List<INode>> VariableDeclarators = Tok(Token.Identifier)
            .SeparatedAtLeastOnce(Tok(Token.Comma))
            .Select(nodes => { return nodes.Select(data => new VariableDeclaratorNode(data)).Cast<INode>().ToList(); })
            .Labelled("VariableDeclarators");

        public static readonly Parser<TokenData, INode> LocalDeclaration
            = Map((type, name) =>
            {
                var data = new PredefinedTypeNode(type);

                return (INode) new LocalDeclarationNode(data, name);
            }, Tok(Token.Keyword), VariableDeclarators).Labelled("Local Declaration");

        public static readonly Parser<TokenData, INode> LocalDeclarationStatement
            = LocalDeclaration
                .Before(Tok(Token.Semicolon))
                .Select<INode>(node => new LocalDeclarationStatementNode((LocalDeclarationNode) node))
                .Labelled("Local Declaration Statement");
        
        private static readonly Parser<TokenData, INode> _statementValid
            = OneOf(_assignment);

        public static readonly Parser<TokenData, INode> Statement
            = _statementValid
                .Before(Tok(Token.Semicolon))
                .Select<INode>(node => new ExpressionStatementNode(node))
                .Labelled("Statement");

        public static readonly Parser<TokenData, List<INode>> Statements
            = OneOf(Statement,LocalDeclarationStatement).Many()
                .Select<List<INode>>(nodes => nodes.ToList())
                .Labelled("Statements");


        public static INode ParseOrThrowExpression(List<TokenData> input)
            => _expr.ParseOrThrow(input);
    }


}