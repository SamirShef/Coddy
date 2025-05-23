using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class VariableDeclarationStatement(VariableStorage variableStorage, string name, TypeValue type, IExpression? expression) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly string name = name;
    private readonly TypeValue type = type;
    private readonly IExpression? expression = expression;

    public void Execute()
    {
        IValue? value = expression?.Evaluate();
        if (value != null)
        {
            TypeValue exprType = value.Type;

            if (!Parser.Parser.IsTypeCompatible(type, exprType)) throw new Exception($"Невозможно преобразовать тип {exprType} в тип {type}");
        }

        variableStorage.Declare(name, type, value);
    }
}
