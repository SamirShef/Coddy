using Core.Lexer;
using Core.Values;

namespace Core.Expressions;

public class BinaryExpression(Token op, IExpression left, IExpression right) : IExpression
{
    private readonly Token op = op;
    private readonly IExpression left = left;
    private readonly IExpression right = right;

    public IValue Evaluate()
    {
        IValue leftVal = left.Evaluate();
        IValue rightVal = right.Evaluate();

        return op.Type switch
        {
            TokenType.Plus => leftVal.Add(rightVal),
            TokenType.Minus => leftVal.Subtract(rightVal),
            TokenType.Multiply => leftVal.Multiply(rightVal),
            TokenType.Divide => leftVal.Divide(rightVal),
            TokenType.Modulo => leftVal.Modulo(rightVal),
            TokenType.Greater => leftVal.Greater(rightVal),
            TokenType.GreaterEqual => leftVal.GreaterEqual(rightVal),
            TokenType.Less => leftVal.Less(rightVal),
            TokenType.LessEqual => leftVal.LessEqual(rightVal),
            TokenType.Equals => leftVal.Equals(rightVal),
            TokenType.NotEquals => leftVal.NotEquals(rightVal),
            TokenType.And => leftVal.And(rightVal),
            TokenType.Or => leftVal.Or(rightVal),
            _ => throw new Exception($"Неизвестный токен оператора: '{op.Value}'."),
        };
    }
}
