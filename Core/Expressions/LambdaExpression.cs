namespace Core.Expressions;

public class LambdaExpression(List<(string, string)> parameters, IExpression expression) : IExpression
{
    public List<(string, string)> Parameters { get; } = parameters;
    public IExpression Expression { get; } = expression;
}
