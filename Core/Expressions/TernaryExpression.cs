using Core.Values;

namespace Core.Expressions;

public class TernaryExpression(IExpression conditionExpression, IExpression trueExpression, IExpression falseExpression) : IExpression
{
    public IExpression ConditionExpression { get; } = conditionExpression;
    public IExpression TrueExpression { get; } = trueExpression;
    public IExpression FalseExpression { get; } = falseExpression;

    public IValue Evaluate()
    {
        IValue conditionValue = ConditionExpression.Evaluate();

        if (conditionValue is BoolValue bv)
        {
            if (bv.AsBool()) return TrueExpression.Evaluate();
            else return FalseExpression.Evaluate();
        }

        throw new Exception($"Условное выражение должно иметь тип boolean. Выражение имеет тип {conditionValue.Type}");
    }
}
