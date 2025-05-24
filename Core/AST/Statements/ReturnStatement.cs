using Core.Expressions;
using Core.Runtime;

namespace Core.AST.Statements;

public class ReturnStatement(IExpression? expression = null) : Exception, IStatement
{
    public IExpression? Expression { get; } = expression;

    public void Execute()
    {
        var value = Expression?.Evaluate();
        throw new ReturnException(value);
    }
}
