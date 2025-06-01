using Core.Expressions;
using Core.Runtime;
using Core.Runtime.OOP;
using Core.Values;

namespace Core.AST.Statements;

public class FieldDeclarationStatement(ClassInfo classInfo, string name, string typeValue, TypeValue type, AccessModifier access, IExpression? expression) : IStatement
{
    private readonly ClassInfo classInfo = classInfo;
    private readonly string name = name;
    private readonly TypeValue type = type;
    private readonly AccessModifier access = access;
    private readonly IExpression? expression = expression;

    public string Name { get; } = name;
    public string TypeValue { get; } = typeValue;
    public TypeValue Type { get; } = type;
    public AccessModifier Access { get; } = access;
    public IExpression? Expression { get; } = expression;

    public void Execute()
    {
        IValue value = expression != null ? expression.Evaluate() : Helpers.GetDefaultValue(type);

        if (value.Type != type) throw new Exception($"Объявление невозможно: несоответствие типов ({value.Type} и {type}).");

        FieldInfo fieldInfo = new(type, access, value);
        classInfo.AddField(name, fieldInfo);
    }
}
