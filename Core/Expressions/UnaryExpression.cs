using Core.Lexer;
using Core.Values;

namespace Core.Expressions;

public class UnaryExpression(Token op, IExpression expr) : IExpression
{
    public Token Op { get; } = op;
    public IExpression Expression { get; } = expr;

    public IValue Evaluate()
    {
        IValue value = Expression.Evaluate();
        return Op.Type switch
        {
            TokenType.Minus => value.Multiply(new IntValue(-1)),
            TokenType.Not => value.NotEquals(new BoolValue(true)),
            _ => throw new Exception($"Неизвестный токен унарного оператора: '{Op.Value}'.")
        };
    }
}
