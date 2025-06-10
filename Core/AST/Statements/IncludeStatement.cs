namespace Core.AST.Statements;

public class IncludeStatement(string libraryPath) : IStatement
{
    public string LibraryPath { get; } = libraryPath;
}