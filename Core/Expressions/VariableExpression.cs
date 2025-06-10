namespace Core.Expressions;

public class VariableExpression(string name) : IExpression
{
    public string Name { get; } = name;
}
