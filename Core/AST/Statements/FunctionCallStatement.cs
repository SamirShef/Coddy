using Core.Expressions;
using Core.Runtime.Functions;

namespace Core.AST.Statements;

public class FunctionCallStatement(FunctionStorage storage, string name, List<IExpression> args) : IStatement
{
    public void Execute()
    {
        var function = storage.Get(name);
        var evaluatedArgs = args.ConvertAll(arg => arg.Evaluate());
        function.Execute([.. evaluatedArgs]);
    }
}
