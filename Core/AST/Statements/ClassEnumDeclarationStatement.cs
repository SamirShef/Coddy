using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class ClassEnumDeclarationStatement(AccessModifier access, string name, List<EnumMember> members) : IStatement
{
    public AccessModifier Access { get; } = access;
    public string Name { get; } = name;
    public List<EnumMember> Members { get; } = members;
}
