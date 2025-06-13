using Core.Expressions;

namespace Core.AST.Statements;

public class ThrowStatement(IExpression expression) : IStatement
{
    public IExpression Expression { get; } = expression;
}
