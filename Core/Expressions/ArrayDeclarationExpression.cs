namespace Core.Expressions;

public class ArrayDeclarationExpression(List<IExpression> expressions, string typeExpression) : IExpression
{
    public List<IExpression> Expressions { get; } = expressions;
    public string TypeExpression { get; } = typeExpression;
}
