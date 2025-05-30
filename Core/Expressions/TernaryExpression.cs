using Core.Values;

namespace Core.Expressions;

public class TernaryExpression(IExpression conditionExpression, IExpression trueExpression, IExpression falseExpression) : IExpression
{
    public IValue Evaluate()
    {
        IValue conditionValue = conditionExpression.Evaluate();

        if (conditionValue is BoolValue bv)
        {
            if (bv.AsBool()) return trueExpression.Evaluate();
            else return falseExpression.Evaluate();
        }

        throw new Exception($"Условное выражение должно иметь тип boolean. Выражение имеет тип {conditionValue.Type}");
    }
}
