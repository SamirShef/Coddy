using Core.Expressions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class FieldDeclarationStatement(string name, bool isConstant, List<string> typeExpressions, AccessModifier access, IExpression? expression, List<string> modifiers, bool hasGetter, bool hasSetter) : IStatement
{
    public string Name { get; } = name;
    public bool IsConstant { get; } = isConstant;
    public List<string> TypeExpressions { get; } = typeExpressions;
    public AccessModifier Access { get; } = access;
    public IExpression? Expression { get; } = expression;
    public List<string> Modifiers { get; } = modifiers;
    public bool HasGetter { get; } = hasGetter;
    public bool HasSetter { get; } = hasSetter;
}
