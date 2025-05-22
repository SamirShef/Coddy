using Core.Runtime;
using Core.Values;
using System.Data.Common;

namespace Core.AST.Statements;

public class AssignmentStatement(VariableStorage variableStorage, string name, IValue newValue) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly string name = name;
    private readonly IValue newValue = newValue;

    public void Execute()
    {
        if (!variableStorage.Exist(name)) throw new Exception($"Присваивание невозможно: переменная/поле с именем '{name}' не существует в текущем контексте.");

        VariableInfo variableInfo = variableStorage.Get(name);

        variableStorage.Set(name, variableInfo.Type, newValue);
    }
}
