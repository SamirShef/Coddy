using Core.Expressions;

namespace Core.AST.Statements;

public class ArrayDeclarationStatement(string name, IExpression? size, List<string> typeExpressions, IExpression? expression = null) : IStatement
{
    public string Name { get; } = name;
    public IExpression? Size { get; } = size;
    public List<string> TypeExpressions { get; set; } = typeExpressions;
    public IExpression? Expression { get; } = expression;
}
