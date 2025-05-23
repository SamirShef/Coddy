using Core.Expressions;
using Core.Values;

namespace Core.AST.Statements;

public class IfElseStatement(IExpression condition, IStatement ifBlock, IStatement? elseBlock) : IStatement
{
    private readonly IExpression condition = condition;
    private readonly IStatement ifBlock = ifBlock;
    private readonly IStatement? elseBlock = elseBlock;

    public void Execute()
    {
        IValue conditionVal = condition.Evaluate();
        if (conditionVal is not BoolValue bv) throw new Exception($"Условное выражение должно иметь тип boolean. Выражение имеет тип {conditionVal.Type}");

        if (bv.AsBool()) ifBlock.Execute();
        else elseBlock?.Execute();
    }
}
