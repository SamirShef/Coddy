namespace Core.Expressions;

public class ArrayExpression(string name, IExpression index) : IExpression
{
    public string Name { get; } = name;
    public IExpression Index { get; } = index;
}
