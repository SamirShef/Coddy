namespace Core.Expressions;

public class FieldExpression(IExpression target, string name, IExpression? start = null) : IExpression
{
    public IExpression Target { get; } = target;
    public string Name { get; } = name;
    public IExpression? Start { get; } = start;
}
