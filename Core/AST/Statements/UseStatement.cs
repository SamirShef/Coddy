using Core.Expressions;

namespace Core.AST.Statements;

public class UseStatement(IExpression filePathExpression) : IStatement
{
    public IExpression FilePathExpression { get; } = filePathExpression;
}
