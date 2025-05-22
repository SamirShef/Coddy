using Core.Lexer;
using Core.Values;

namespace Core.Expressions;

public class UnaryExpression(Token op, IExpression expr) : IExpression
{
    public IValue Evaluate()
    {
        IValue value = expr.Evaluate();
        return op.Type switch
        {
            TokenType.Minus => value.Subtract(new IntValue(-1)),
            TokenType.Not => value.NotEquals(new BoolValue(true)),
            _ => throw new Exception($"Неизвестный токен унарного оператора: '{op.Value}'.")
        };
    }
}
