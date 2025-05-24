using Core.Runtime;
using Core.Values;

namespace Core.Expressions;

public class NewExpression(ClassStorage classStorage, string name) : IExpression
{
    private readonly ClassStorage classStorage = classStorage;
    private readonly string name = name;

    public IValue Evaluate() => new ObjectValue(classStorage.Get(name));
}
