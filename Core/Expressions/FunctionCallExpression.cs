namespace Core.Expressions;

public class FunctionCallExpression(string name, List<IExpression> args) : IExpression
{
    public string Name { get; } = name;
    public List<IExpression> Args { get; } = args;
}
