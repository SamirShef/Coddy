using Core.Runtime;
using Core.Values;

namespace Core.Expressions;

public class VariableExpression(VariableStorage storage, string name) : IExpression
{
    public string Name { get; } = name;
    public IValue Evaluate() => storage.Get(Name).Value;
}
