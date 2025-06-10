namespace Core.Expressions;

public class ArrayFieldExpression(IExpression target, string name, IExpression index) : IExpression
{
    public IExpression Target { get; } = target;
    public string Name { get; } = name;
    public IExpression Index { get; } = index;
}
