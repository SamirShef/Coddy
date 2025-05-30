using Core.Values;

namespace Core.Expressions;

public class MethodCallExpression(IExpression target, string methodName, List<IExpression> arguments) : IExpression
{
    private readonly IExpression target = target;
    private readonly string methodName = methodName;
    private readonly List<IExpression> arguments = arguments;

    public IValue Evaluate()
    {
        IValue targetValue = target.Evaluate();
        
        if (targetValue is not ClassValue cv) throw new Exception($"Невозможно вызывать метод: тип {targetValue.Type} не является объектом.");

        IValue result = cv.Instance.CallMethod(methodName, [.. arguments.Select(arg => arg.Evaluate())]);
        
        if (result is ClassValue resultClass) return resultClass;
        
        if (result is VoidValue) return targetValue;

        return result;
    }
}