using Core.Runtime.Functions;

namespace Core.AST.Statements;

public class FunctionDeclarationStatement(FunctionStorage functionStorage, string name, IFunction function) : IStatement
{
    public void Execute() => functionStorage.Declare(name, function);
}
