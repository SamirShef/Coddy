using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class VariableDeclarationStatement(VariableStorage variableStorage, string name, TypeValue type, IExpression expression) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly string name = name;
    private readonly TypeValue type = type;
    private readonly IExpression expression = expression;

    public void Execute() => variableStorage.Declare(name, type, expression.Evaluate());
}
