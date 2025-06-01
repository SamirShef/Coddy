using System.IO;
using Core.AST.Statements;
using Core.Lexer;
using Core.Parser;
using Core.Translator;

namespace IDE;

public class Compiler
{
    public static void Execute(string source)
    {
        Lexer lexer = new(source);

        Parser parser = new([.. lexer.Tokenize()]);
        List<IStatement> statements = parser.Parse();

        //foreach (IStatement statement in statements) statement.Execute();
        
        Console.WriteLine("Translating to C#...");
        string translatedCode = Translator.Translate(statements);

        File.WriteAllText($"{Directory.GetCurrentDirectory()}\\Generated\\Program.cs", translatedCode);
        Console.WriteLine("Translating ended, executing...");

        CodeRunner.RunGeneratedCode(translatedCode);
    }
}
