using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class IncludeStatement(ClassStorage classStorage, string libraryPath) : IStatement
{
    private readonly ClassStorage classStorage = classStorage;
    private readonly string libraryPath = libraryPath;

    public string LibraryPath { get; } = libraryPath;

    public void Execute()
    {
        string fullPath = Path.Combine("Libraries", libraryPath);
        if (!File.Exists(fullPath)) throw new Exception($"Библиотека не найдена: {fullPath}");

        string source = File.ReadAllText(fullPath);
        var lexer = new Lexer.Lexer(source);
        var parser = new Parser.Parser([.. lexer.Tokenize()]);
        var statements = parser.Parse();

        foreach (var statement in statements)
        {
            if (statement is ClassDeclarationStatement classDeclaration)
            {
                classDeclaration.Execute();
            }
            else
            {
                throw new Exception($"В библиотеке могут быть только объявления классов. Найден недопустимый оператор: {statement.GetType().Name}");
            }
        }
    }
} 