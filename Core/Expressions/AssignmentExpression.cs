namespace Core.Expressions;

public class AssignmentExpression(IExpression target, string name, IExpression expression) : IExpression
{
    public IExpression TargetExpression { get; } = target;
    public string Name { get; } = name;
    public IExpression Expression { get; } = expression;
}
