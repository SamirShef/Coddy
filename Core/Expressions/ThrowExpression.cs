namespace Core.Expressions;

public class ThrowExpression(IExpression expression) : IExpression
{
    public IExpression Expression { get; } = expression;
}
