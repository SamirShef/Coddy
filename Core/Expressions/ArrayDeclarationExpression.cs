namespace Core.Expressions;

public class ArrayDeclarationExpression(List<IExpression> expressions) : IExpression
{
    public List<IExpression> Expressions { get; } = expressions;
}
