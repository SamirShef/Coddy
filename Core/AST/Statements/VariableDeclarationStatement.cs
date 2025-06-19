using Core.Expressions;

namespace Core.AST.Statements;

public class VariableDeclarationStatement(string name, bool isConstant, List<string> typeExpressions, IExpression? expression) : IStatement
{
    public string Name { get; } = name;
    public bool IsConstant { get; } = isConstant;
    public List<string> TypeExpressions { get; } = typeExpressions;
    public IExpression? Expression { get; } = expression;
}
