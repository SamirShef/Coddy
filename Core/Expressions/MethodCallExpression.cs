using Core.Values;

namespace Core.Expressions;

public class MethodCallExpression(IExpression target, string methodName, List<IExpression> arguments) : IExpression
{
    private readonly IExpression target = target;
    private readonly string methodName = methodName;
    private readonly List<IExpression> arguments = arguments;

    public IExpression Target { get; } = target;
    public string MethodName { get; } = methodName;
    public List<IExpression> Args { get; } = arguments;

    public IValue Evaluate()
    {
        IValue targetValue = target.Evaluate();
        
        if (targetValue is not ClassValue cv) throw new Exception($"Невозможно вызывать метод: тип {targetValue.Type} не является объектом.");

        bool isThisContext = target is VariableExpression ve && ve.Name == "this";
        IValue result = cv.Instance.CallMethod(methodName, [.. arguments.Select(arg => arg.Evaluate())], isThisContext);
        
        if (result is ClassValue resultClass) return resultClass;
        
        if (result is VoidValue) return targetValue;

        return result;
    }
}