using Core.Expressions;
using Core.Runtime.OOP;
using Core.Values;

namespace Core.AST.Statements;

public class FieldArrayDeclarationStatement(ClassInfo classInfo, string name, string typeValue, TypeValue type, TypeValue primaryType, AccessModifier access, IExpression size, List<IExpression> expressions, bool isStatic = false) : IStatement
{
    public string Name { get; } = name;
    public string TypeValue { get; } = typeValue;
    public TypeValue Type { get; } = type;
    public TypeValue PrimaryType { get; } = primaryType;
    public AccessModifier Access { get; } = access;
    public IExpression Size { get; } = size;
    public List<IExpression> Expressions { get; } = expressions;
    public bool IsStatic { get; } = isStatic;

    public void Execute()
    {
        IValue sizeValue = Size != null ? Size.Evaluate() : new IntValue(0);
        if (sizeValue is not IntValue iv) throw new Exception($"Размер массива должен быть целым числом. Сейчас размер массива имеет тип {sizeValue.Type}.");
        if (iv.AsInt() < 0) throw new Exception($"Размер массива должен быть неотрицательным числом. Сейчас указанный размер массива равен {iv.AsInt()}.");

        IValue[] values = [.. Expressions.Select(expr => expr.Evaluate())];

        if (values.Length != iv.AsInt() && iv.AsInt() != 0) throw new Exception($"Переданное количество элементов не соответствует указанному размеру массива (размер: {iv.AsInt()}, количество элементов: {values.Length}).");

        foreach (IValue value in values) if (value.Type != Type) throw new Exception($"Типы элементов не соответствуют основному типу массива {Type}.");

        classInfo.AddField(Name, new FieldInfo(type, Access, new ArrayValue(values, Type), IsStatic));
    }
}
