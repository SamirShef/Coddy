using Core.Runtime.OOP;
using Core.Values;

namespace Core.Expressions;

public class FieldExpression(string name, IExpression targetExpression) : IExpression
{
    public IValue Evaluate()
    {
        IValue targetValue = targetExpression.Evaluate();
        if (targetValue is not ClassValue value) throw new Exception($"Невозможно получить значение поля: целевой объект не является классом.");

        return value.Instance.GetFieldValue(name);
    }
}
