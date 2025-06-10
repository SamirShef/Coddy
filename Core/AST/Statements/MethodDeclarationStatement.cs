using Core.Runtime.Functions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class MethodDeclarationStatement(string methodName, UserFunction method, AccessModifier access, bool isStatic = false) : IStatement
{
    public string MethodName { get; } = methodName;
    public UserFunction Method { get; } = method;
    public AccessModifier Access { get; } = access;
    public bool IsStatic { get; } = isStatic;
} 