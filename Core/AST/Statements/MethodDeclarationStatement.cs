using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class MethodDeclarationStatement(ClassStorage classStorage, string className, AccessModifier access, string methodName, TypeValue returnType, List<(string, TypeValue)> parameters, IStatement body) : IStatement
{
    private readonly ClassStorage classStorage = classStorage;
    private readonly string className = className;
    public AccessModifier Access { get; } = access;
    public string Name { get; } = methodName;
    public TypeValue ReturnType { get; } = returnType;
    public List<(string Name, TypeValue Type)> Parameters { get; } = parameters;
    public IStatement Body { get; } = body;

    public void Execute()
    {
        var classInfo = classStorage.Get(className);
        var methodInfo = new MethodInfo(Access, ReturnType, Parameters, Body);
        classInfo.AddMethod(Name, methodInfo);
    }
}
