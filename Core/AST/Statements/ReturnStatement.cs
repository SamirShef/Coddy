using Core.Expressions;

namespace Core.AST.Statements;

public class ReturnStatement(IExpression? expression = null) : Exception, IStatement
{
    public IExpression? Expression { get; } = expression;
}
