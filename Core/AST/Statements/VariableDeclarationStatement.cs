using Core.Expressions;

namespace Core.AST.Statements;

public class VariableDeclarationStatement(string name, List<string> typeExpressions, IExpression? expression) : IStatement
{
    public string Name { get; } = name;
    public List<string> TypeExpressions { get; } = typeExpressions;
    public IExpression? Expression { get; } = expression;
}
