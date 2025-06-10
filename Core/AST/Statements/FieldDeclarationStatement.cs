using Core.Expressions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class FieldDeclarationStatement(string name, List<string> typeExpressions, AccessModifier access, IExpression? expression, bool isStatic = false) : IStatement
{
    public string Name { get; } = name;
    public List<string> TypeExpressions { get; } = typeExpressions;
    public AccessModifier Access { get; } = access;
    public IExpression? Expression { get; } = expression;
    public bool IsStatic { get; } = isStatic;
}
