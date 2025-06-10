namespace Core.Expressions;

public class NewClassExpression(string name, List<IExpression>? arguments = null) : IExpression
{
    public string Name { get; } = name;
    public List<IExpression>? Args { get; } = arguments;
}
