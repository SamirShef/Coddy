namespace Core.Expressions;

public class TernaryExpression(IExpression conditionExpression, IExpression trueExpression, IExpression falseExpression) : IExpression
{
    public IExpression ConditionExpression { get; } = conditionExpression;
    public IExpression TrueExpression { get; } = trueExpression;
    public IExpression FalseExpression { get; } = falseExpression;
}
