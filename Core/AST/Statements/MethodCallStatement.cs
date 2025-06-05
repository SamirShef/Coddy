using Core.Expressions;

namespace Core.AST.Statements;

public class MethodCallStatement(IExpression expression) : IStatement
{
    public IExpression Expression { get; } = expression;

    public void Execute() => Expression.Evaluate();
}