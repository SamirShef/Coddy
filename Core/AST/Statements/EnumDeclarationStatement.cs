using Core.Expressions;

namespace Core.AST.Statements;

public class EnumDeclarationStatement(string name, List<EnumMember> members) : IStatement
{
    public string Name { get; } = name;
    public List<EnumMember> Members { get; } = members;
}

public class EnumMember(string name, IExpression? expression)
{
    public string Name { get; } = name;
    public IExpression? Expression { get; } = expression;
}