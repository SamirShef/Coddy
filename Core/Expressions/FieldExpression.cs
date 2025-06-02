using Core.Values;

namespace Core.Expressions;

public class FieldExpression(IExpression target, string name) : IExpression
{
    public IExpression Target { get; } = target;
    public string Name { get; } = name;

    public IValue Evaluate()
    {
        IValue targetValue = Target.Evaluate();
        if (targetValue is not ClassValue cv) throw new Exception($"Невозможно получить значение поля: тип {targetValue.Type} не является объектом.");

        bool isThisContext = Target is VariableExpression ve && ve.Name == "this";
        return cv.Instance.GetFieldValue(Name, isThisContext);
    }
}
