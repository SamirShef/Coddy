using Core.Expressions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class FieldArrayDeclarationStatement(string name, List<string> typeExpressions, AccessModifier access, IExpression size, List<string> modifiers, bool hasGetter, bool hasSetter, IExpression? expression = null) : IStatement
{
    public string Name { get; } = name;
    public List<string> TypeExpressions { get; } = typeExpressions;
    public AccessModifier Access { get; } = access;
    public IExpression Size { get; } = size;
    public List<string> Modifiers { get; } = modifiers;
    public bool HasGetter { get; } = hasGetter;
    public bool HasSetter { get; } = hasSetter;
    public IExpression? Expression { get; } = expression;
}
