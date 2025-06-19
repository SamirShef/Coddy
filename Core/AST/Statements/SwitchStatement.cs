using Core.Expressions;

namespace Core.AST.Statements;

public class SwitchStatement(IExpression expression, List<(IExpression, IStatement)> cases, (IExpression, IStatement)? defaultCase) : IStatement
{
    public IExpression Expression { get; } = expression;
    public List<(IExpression, IStatement)> Cases { get; } = cases;
    public (IExpression, IStatement)? DefaultCase { get; } = defaultCase;
}
