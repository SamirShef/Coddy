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
        
        if (targetValue is not ClassValue cv)
        {
            throw new Exception($"Попытка вызвать метод {methodName} на не-объекте типа {targetValue.Type}");
        }

        return cv.Instance.CallMethod(methodName, [.. arguments.Select(arg => arg.Evaluate())]);
    }
} 