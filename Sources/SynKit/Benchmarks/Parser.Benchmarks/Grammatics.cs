using System;
using Yoakke.SynKit.Lexer;
using Yoakke.SynKit.Lexer.Attributes;
using Yoakke.SynKit.Parser.Attributes;

namespace Yoakke.SynKit.Parser.Benchmarks;

public enum TokenType
{
    [Error] Error,
    [End] End,
    [Ignore] [Regex(Regexes.Whitespace)] Whitespace,

    [Token("(")] OpenParen,
    [Token(")")] CloseParen,

    [Token("+")] Add,
    [Token("-")] Sub,
    [Token("*")] Mul,
    [Token("/")] Div,
    [Token("%")] Mod,
    [Token("^")] Exp,

    [Token(";")] Semicol,

    [Regex(Regexes.IntLiteral)] IntLit,
}

[Lexer(typeof(TokenType))]
public partial class Lexer
{
    public List<Token<TokenType>> LexAll()
    {
        var list = new List<Token<TokenType>>();
        while (true)
        {
            var token = Next();
            list.Add(token);
            if (token.Kind == TokenType.End) break;
        }
        return list;
    }
}

[Parser(typeof(TokenType))]
public partial class Parser
{
    [Rule("program: expression ';'")]
    public static int Program(int n, IToken _) => n;

    [Right("^")]
    [Left("*", "/", "%")]
    [Left("+", "-")]
    [Rule("expression")]
    public static int BinOp(int a, IToken op, int b) => op.Text switch
    {
        "^" => (int)Math.Pow(a, b),
        "*" => a * b,
        "/" => a / b,
        "%" => a % b,
        "+" => a + b,
        "-" => a - b,
        _ => throw new NotImplementedException(),
    };

    [Rule("expression : '(' expression ')'")]
    public static int Grouping(IToken _1, int n, IToken _2) => n;

    [Rule("expression : IntLit")]
    public static int IntLit(IToken token) => int.Parse(token.Text);
}