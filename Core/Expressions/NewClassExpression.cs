using Core.Runtime.OOP;
using Core.Values;

namespace Core.Expressions;

public class NewClassExpression(ClassStorage classStorage, string name) : IExpression
{
    public IValue Evaluate() => new ClassValue(new ClassInstance(classStorage.Get(name)));
}
