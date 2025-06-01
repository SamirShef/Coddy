using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class AssignmentStatement(VariableStorage variableStorage, string name, IExpression newExpression) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly string name = name;
    private readonly IExpression newExpression = newExpression;

    public string Name { get; } = name;
    public IExpression NewExpression { get; } = newExpression;

    public void Execute()
    {
        if (!variableStorage.Exist(name)) throw new Exception($"Присваивание невозможно: переменная с именем '{name}' не существует в текущем контексте.");
        VariableInfo variableInfo = variableStorage.Get(name);

        IValue newValue = newExpression.Evaluate();

        if (!Parser.Parser.IsTypeCompatible(variableInfo.Type, newValue.Type)) throw new Exception($"Невозможно преобразовать тип {newValue.Type} в тип {variableInfo.Type}.");

        variableStorage.Set(name, variableInfo.Type, newValue);
    }
}
