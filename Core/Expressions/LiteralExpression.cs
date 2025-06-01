using Core.Values;

namespace Core.Expressions;

public class LiteralExpression(IValue value) : IExpression
{
    public IValue Value { get; } = value;

    public IValue Evaluate() => Value;
}
