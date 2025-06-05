using Core.Runtime.Functions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class MethodDeclarationStatement(ClassInfo classInfo, string methodName, UserFunction method, AccessModifier access, bool isStatic = false) : IStatement
{
    private readonly ClassInfo classInfo = classInfo;
    private readonly string methodName = methodName;
    private readonly UserFunction method = method;
    private readonly AccessModifier access = access;
    private readonly bool isStatic = isStatic;

    public string MethodName { get; } = methodName;
    public UserFunction Method { get; } = method;
    public AccessModifier Access { get; } = access;
    public bool IsStatic { get; } = isStatic;

    public void Execute() => classInfo.AddMethod(methodName, new MethodInfo(access, method, isStatic));
} 