using Core.Expressions;
using Core.Runtime.Functions;
using Core.Values;

namespace Core.AST.Statements;

public class FunctionCallStatement(FunctionStorage storage, string name, List<IExpression> args) : IStatement
{
    public string Name { get; } = name;
    public List<IExpression> Args { get; } = args;

    public void Execute()
    {
        IFunction function = storage.Get(Name);
        List<IValue> evaluatedArgs = Args.ConvertAll(arg => arg.Evaluate());
        function.Execute([.. evaluatedArgs]);
    }
}
