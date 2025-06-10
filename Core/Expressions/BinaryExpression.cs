using Core.Lexer;

namespace Core.Expressions;

public class BinaryExpression(Token op, IExpression left, IExpression right) : IExpression
{
    public Token Op { get; } = op;
    public IExpression Left { get; } = left;
    public IExpression Right { get; } = right;
}
