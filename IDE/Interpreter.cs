using System.IO;
using Core.Lexer;

namespace IDE;

public class Interpreter
{
    private readonly static string source = File.ReadAllText("your\\path\\to\\file");

    public static void Execute()
    {
        Lexer lexer = new(source);
        List<Token> tokens = [.. lexer.Tokenize()];

        foreach (Token token in tokens) Console.WriteLine(token);
    }
}
