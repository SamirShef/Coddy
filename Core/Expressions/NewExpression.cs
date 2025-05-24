using Core.Runtime;
using Core.Values;

namespace Core.Expressions;

public class NewExpression(ClassStorage classStorage, string name) : IExpression
{
    private readonly ClassStorage classStorage = classStorage;
    private readonly string name = name;

    public IValue Evaluate()
    {
        ObjectValue obj = new(classStorage.Get(name));
        obj.InitializeFields();
        return obj;
    }
}
