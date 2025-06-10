using Core.Lexer;

namespace Core.Expressions;

public class UnaryExpression(Token op, IExpression expr) : IExpression
{
    public Token Op { get; } = op;
    public IExpression Expression { get; } = expr;
}
