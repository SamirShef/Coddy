using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class DoWhileLoopStatement(VariableStorage variableStorage, IExpression condition, IStatement block) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly IExpression condition = condition;
    private readonly IStatement block = block;

    public void Execute()
    {
        IValue conditionVal = condition.Evaluate();
        if (conditionVal is not BoolValue bv) throw new Exception($"Условное выражение должно иметь тип boolean. Выражение имеет тип {conditionVal.Type}");

        do
        {
            variableStorage.EnterScope();
            try
            {
                try { block.Execute(); }
                catch (BreakStatement) { break; }
                catch (ContinueStatement) { continue; }
            }
            finally { variableStorage.ExitScope(); }
        }
        while (IsConditionTrue(condition.Evaluate()));
    }

    private bool IsConditionTrue(IValue value)
    {
        if (value is BoolValue bv) return bv.AsBool();
        throw new Exception($"Условное выражение должно иметь тип boolean. Выражение имеет тип {value.Type}");
    }
}
