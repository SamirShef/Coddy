using Core.Expressions;

namespace Core.AST.Statements;

public class FunctionCallStatement(string name, List<IExpression> args) : IStatement
{
    public string Name { get; } = name;
    public List<IExpression> Args { get; } = args;
}
