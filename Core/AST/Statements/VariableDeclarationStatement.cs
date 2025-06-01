using Core.Expressions;
using Core.Runtime;
using Core.Runtime.OOP;
using Core.Values;

namespace Core.AST.Statements;

public class VariableDeclarationStatement(VariableStorage variableStorage, ClassStorage classStorage, string name, string typeValue, TypeValue type, IExpression? expression) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly string name = name;
    private readonly string typeValue = typeValue;
    private readonly TypeValue type = type;
    private readonly IExpression? expression = expression;

    public string Name { get; } = name;
    public string TypeValue { get; } = typeValue;
    public TypeValue Type { get; } = type;
    public IExpression? Expression { get; } = expression;

    public void Execute()
    {
        if (type == Values.TypeValue.Class && !classStorage.Exist(typeValue)) throw new Exception($"Объявление невозможно: класс с именем '{typeValue}' не существует.");
        IValue? value = expression?.Evaluate();
        if (value != null)
        {
            TypeValue exprType = value.Type;

            if (!Parser.Parser.IsTypeCompatible(type, exprType)) throw new Exception($"Невозможно преобразовать тип {exprType} в тип {type}.");
        }

        variableStorage.Declare(name, type, value);
    }
}
