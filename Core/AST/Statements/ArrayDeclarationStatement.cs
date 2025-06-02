using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class ArrayDeclarationStatement(VariableStorage variableStorage, string name, IExpression? size, string typeValue, TypeValue baseType, List<IExpression> expressions) : IStatement
{
    public string Name { get; } = name;
    public IExpression? Size { get; } = size;
    public string TypeValue { get; } = typeValue;
    public TypeValue Type { get; } = baseType;
    public List<IExpression> Expressions { get; } = expressions;

    public void Execute()
    {
        IValue sizeValue = Size != null ? Size.Evaluate() : new IntValue(0);
        if (sizeValue is not IntValue iv) throw new Exception($"Размер массива должен быть целым числом. Сейчас размер массива имеет тип {sizeValue.Type}.");
        if (iv.AsInt() < 0) throw new Exception($"Размер массива должен быть неотрицательным числом. Сейчас указанный размер массива равен {iv.AsInt()}.");

        IValue[] values = [.. Expressions.Select(expr => expr.Evaluate())];

        if (values.Length != iv.AsInt() && iv.AsInt() != 0) throw new Exception($"Переданное количество элементов не соответствует указанному размеру массива (размер: {iv.AsInt()}, количество элементов: {values.Length}).");

        foreach (IValue value in values) if (value.Type != Type) throw new Exception($"Типы элементов не соответствуют основному типу массива {Type}.");

        variableStorage.Declare(Name, Type, new ArrayValue(values, Type));
    }
}
