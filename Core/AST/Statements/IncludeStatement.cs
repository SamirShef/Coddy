using Core.Expressions;

namespace Core.AST.Statements;

public class IncludeStatement(IExpression libraryPathExpression) : IStatement
{
    public IExpression LibraryPathExpression { get; } = libraryPathExpression;
}