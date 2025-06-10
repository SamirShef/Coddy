using Core.Expressions;

namespace Core.AST.Statements;

public class ArrayAssignmentStatement(string name, IExpression index, IExpression expression) : IStatement
{
    public string Name { get; } = name;
    public IExpression Index { get; } = index;
    public IExpression Expression { get; } = expression;
}
