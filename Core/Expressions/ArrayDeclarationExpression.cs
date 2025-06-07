using Core.Values;

namespace Core.Expressions;

public class ArrayDeclarationExpression(List<IExpression> expressions, string? elementsTypeValue = null, TypeValue? elementsType = null) : IExpression
{
    public List<IExpression> Expressions { get; } = expressions;
    public TypeValue ElementsType { get; } = elementsType ?? expressions[0].Evaluate().Type;
    public string ElementsTypeValue { get; } = elementsTypeValue ?? expressions[0].Evaluate().Type.ToString().ToLower();

    public IValue Evaluate()
    {
        IValue[] values = [.. Expressions.Select(expr => expr.Evaluate())];

        foreach (IValue value in values) if (value.Type != ElementsType) throw new Exception($"Элементы инициализатора массива не имеют один тип/не совпадают с типом {ElementsType}.");

        return new ArrayValue(values, ElementsType);
    }
}
