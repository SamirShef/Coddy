using Core.Runtime;
using Core.Values;

namespace Core.Expressions;

public class VariableExpression(VariableStorage storage, string name) : IExpression
{
    public IValue Evaluate() => storage.Get(name).Value;
}
