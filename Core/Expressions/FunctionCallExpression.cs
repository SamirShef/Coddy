using Core.Runtime.Functions;
using Core.Values;

namespace Core.Expressions;

public class FunctionCallExpression(FunctionStorage storage, string name, List<IExpression> args) : IExpression
{
    public string Name { get; } = name;
    public List<IExpression> Args { get; } = args;

    public IValue Evaluate()
    {
        IFunction function = storage.Get(Name);
        return function.Execute([.. Args.ConvertAll(arg => arg.Evaluate())]);
    }
}
