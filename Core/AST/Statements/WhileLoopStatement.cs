using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class WhileLoopStatement(VariableStorage variableStorage, IExpression condition, IStatement block) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly IExpression condition = condition;
    private readonly IStatement block = block;

    public void Execute()
    {
        IValue conditionVal = condition.Evaluate();
        if (conditionVal is not BoolValue bv) throw new Exception($"Условное выражение должно иметь тип boolean. Выражение имеет тип {conditionVal.Type}");

        while (IsConditionTrue(condition.Evaluate()))
        {
            variableStorage.EnterScope();
            try
            {
                try { block.Execute(); }
                catch (BreakStatement) { break; }
                catch (ContinueStatement) { continue; }
                catch (ReturnException) { throw; }
            }
            finally { variableStorage.ExitScope(); }
        }
    }

    private bool IsConditionTrue(IValue value)
    {
        if (value is BoolValue bv) return bv.AsBool();
        throw new Exception($"Условное выражение должно иметь тип boolean. Выражение имеет тип {value.Type}");
    }
}
