using Core.Expressions;

namespace Core.AST.Statements;

public class AssignmentStatement(string name, IExpression newExpression) : IStatement
{
    public string Name { get; } = name;
    public IExpression NewExpression { get; } = newExpression;
}
