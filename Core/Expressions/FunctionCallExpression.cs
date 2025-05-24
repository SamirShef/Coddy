using Core.Runtime.Functions;
using Core.Values;

namespace Core.Expressions;

public class FunctionCallExpression(FunctionStorage storage, string name, List<IExpression> args) : IExpression
{
    public IValue Evaluate()
    {
        var function = storage.Get(name);
        var evaluatedArgs = args.ConvertAll(arg => arg.Evaluate());
        return function.Execute([.. evaluatedArgs]);
    }
}
