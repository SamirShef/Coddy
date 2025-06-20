using System.IO;
using Core.AST.Statements;
using Core.Lexer;
using Core.Parser;
using Core.Translator;

namespace Coddy.IDE;

public class Compiler
{
    public static void Execute(string source, string currentFilePath)
    {
        Lexer lexer = new(source);

        Parser parser = new([.. lexer.Tokenize()]);
        List<IStatement> statements = parser.Parse();
        Console.WriteLine("Трансляция в C#...");
        string translatedCode = Translator.Translate(statements, currentFilePath);

        string generatedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Generated");
        if (!Directory.Exists(generatedPath)) Directory.CreateDirectory(generatedPath);

        string programPath = Path.Combine(generatedPath, "Program.cs");
        File.WriteAllText(programPath, translatedCode);
        Console.WriteLine("Трансляция завершена. Выполнение...");

        CodeRunner.RunGeneratedCode(translatedCode);

        Console.WriteLine($"{new string('-', 10)}\nВыполнение завершено");

        File.Delete(programPath);
    }
}
