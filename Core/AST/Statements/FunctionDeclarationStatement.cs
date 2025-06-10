using Core.Runtime.Functions;

namespace Core.AST.Statements;

public class FunctionDeclarationStatement(string name, UserFunction function) : IStatement
{
    public string Name { get; } = name;
    public UserFunction UserFunction { get; } = function;
}
