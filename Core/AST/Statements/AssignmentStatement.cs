using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class AssignmentStatement(VariableStorage variableStorage, string name, IExpression newExpression) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly string name = name;
    private readonly IExpression newExpression = newExpression;

    public void Execute()
    {
        IValue newValue = newExpression.Evaluate();

        if (name.Contains('.'))
        {
            var parts = name.Split('.');
            var objName = parts[0];
            var fieldName = parts[1];

            if (!variableStorage.Exist(objName)) throw new Exception($"Объект '{objName}' не существует.");

            var obj = variableStorage.Get(objName).Value as ObjectValue ?? throw new Exception($"'{objName}' не является объектом.");

            if (!obj.InstanceFields.ContainsKey(fieldName)) throw new Exception($"Поле '{fieldName}' не найдено в классе.");

            obj.InstanceFields[fieldName] = newValue;
        }
        else
        {
            if (!variableStorage.Exist(name))
                throw new Exception($"Переменная '{name}' не объявлена");

            var variableInfo = variableStorage.Get(name);

            if (!Parser.Parser.IsTypeCompatible(variableInfo.Type, newValue.Type)) throw new Exception($"Несовместимые типы: {variableInfo.Type} и {newValue.Type}");

            variableStorage.Set(name, variableInfo.Type, newValue);
        }
    }
}
