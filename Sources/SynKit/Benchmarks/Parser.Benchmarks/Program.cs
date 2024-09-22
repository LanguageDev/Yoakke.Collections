// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Yoakke.SynKit.Lexer;
using Yoakke.SynKit.Parser;
using Yoakke.SynKit.Parser.Benchmarks;

var a = new SimpleBench();
for (int i = 0; i < 10_000_000; i++)
    a.Parse();
// BenchmarkRunner.Run<SimpleBench>();


 // UNCOMMENT FOR DEBUG
/*
var result = new SimpleBench().Parse();
if (result.IsOk)
{
    Console.WriteLine($" => {result.Ok.Value}");
}
else
{
    var err = result.Error;
    foreach (var element in err.Elements.Values)
    {
        Console.WriteLine($"  expected {string.Join(" or ", element.Expected)} while parsing {element.Context}");
    }
    Console.WriteLine($"  but got {(err.Got == null ? "end of input" : ((IToken<TokenType>)err.Got).Text)}");
}*/

public class SimpleBench
{
    private static string source = "1+1/2+6^8-1-15*2;";

    [Benchmark]
    public ParseResult<int> Parse()
    {
        return new Parser(new Lexer(source)).ParseProgram();
    }
    
    [Benchmark]
    public List<Token<TokenType>> Lex()
    {
        return new Lexer(source).LexAll();
    }
}
