using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class ClassInterfaceDeclarationStatement(AccessModifier access, string name, List<string> implements, List<(string, string, List<(string, string)>)> methods) : IStatement
{
    public AccessModifier Access { get; } = access;
    public string Name { get; } = name;
    public List<string> Implements { get; } = implements;
    public List<(string, string, List<(string, string)>)> Methods { get; } = methods;
}
