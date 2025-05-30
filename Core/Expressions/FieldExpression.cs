using Core.Values;

namespace Core.Expressions;

public class FieldExpression(string name, IExpression targetExpression) : IExpression
{
    public string Name { get; } = name;
    public IExpression TargetExpression { get; } = targetExpression;

    public IValue Evaluate()
    {
        IValue targetValue = TargetExpression.Evaluate();
        if (targetValue is not ClassValue cv) throw new Exception($"Невозможно получить значение поля: тип {targetValue.Type} не является объектом.");

        bool isThisContext = TargetExpression is VariableExpression ve && ve.Name == "this";
        return cv.Instance.GetFieldValue(Name, isThisContext);
    }
}
