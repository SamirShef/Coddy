using Core.AST.Statements;

namespace Core.Runtime.Functions;

public class UserFunction(string name, string returnType, List<(string, string)> parameters, IStatement body, bool isStatic = false) : IFunction
{
    public string Name { get; } = name;
    public string ReturnType { get; } = returnType;
    public List<(string, string)> Parameters { get; } = parameters;
    public IStatement Body { get; } = body;
    public bool IsStatic { get; } = isStatic;
}
