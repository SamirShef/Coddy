using Core.Values;

namespace Core.Expressions;

public class LiteralExpression(IValue value) : IExpression
{
    public IValue Evaluate() => value;
}
