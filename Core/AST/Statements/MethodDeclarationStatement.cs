using Core.Runtime.Functions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class MethodDeclarationStatement(ClassInfo classInfo, string methodName, UserFunction method, AccessModifier access) : IStatement
{
    private readonly ClassInfo classInfo = classInfo;
    private readonly string methodName = methodName;
    private readonly UserFunction method = method;
    private readonly AccessModifier access = access;

    public void Execute() => classInfo.AddMethod(methodName, new MethodInfo(access, method));
} 