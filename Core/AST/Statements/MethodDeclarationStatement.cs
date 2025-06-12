using Core.Runtime.Functions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class MethodDeclarationStatement(string methodName, UserFunction method, AccessModifier access, List<string> modifiers) : IStatement
{
    public string MethodName { get; } = methodName;
    public UserFunction Method { get; } = method;
    public AccessModifier Access { get; } = access;
    public List<string> Modifiers { get; } = modifiers;
} 