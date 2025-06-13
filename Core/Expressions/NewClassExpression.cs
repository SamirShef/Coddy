namespace Core.Expressions;

public class NewClassExpression(string name, string? genericsParameters, List<IExpression>? arguments = null) : IExpression
{
    public string Name { get; } = name;
    public string GenericsParameters { get; } = genericsParameters;
    public List<IExpression>? Args { get; } = arguments;
}
