namespace Core.Expressions;

public class MethodCallExpression(IExpression target, string methodName, List<IExpression> arguments, IExpression? start = null) : IExpression
{
    public IExpression Target { get; } = target;
    public string MethodName { get; } = methodName;
    public List<IExpression> Args { get; } = arguments;
    public IExpression? Start { get; } = start;
}