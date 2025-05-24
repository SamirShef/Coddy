using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class FieldDeclarationStatement(ClassStorage classStorage, string className, AccessModifier access, TypeValue type, string name, IExpression? initializer) : IStatement
{
    private readonly ClassStorage classStorage = classStorage;
    private readonly string className = className;
    public AccessModifier Access { get; } = access;
    public TypeValue Type { get; } = type;
    public string Name { get; } = name;
    public IExpression? Initializer { get; } = initializer;

    public void Execute()
    {
        var classInfo = classStorage.Get(className);
        IValue? value;
        if (Initializer != null) value = Initializer.Evaluate();
        else value = GetDefaultValue(type);
        classInfo.AddField(Name, new FieldInfo(Access, Type, value));
    }

    private IValue GetDefaultValue(TypeValue type) => type switch
    {
        TypeValue.Int => new IntValue(0),
        TypeValue.Float => new FloatValue(0f),
        TypeValue.Double => new DoubleValue(0d),
        TypeValue.Decimal => new DecimalValue(0m),
        TypeValue.String => new StringValue(""),
        TypeValue.Bool => new BoolValue(false),
        _ => throw new Exception($"Не удается получить стандартное значение для типа переменной '{type}'.")
    };
}
