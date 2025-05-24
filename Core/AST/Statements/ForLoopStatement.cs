using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class ForLoopStatement(VariableStorage variableStorage, IStatement indexatorDeclaration, IExpression condition, IStatement iterator, IStatement block) : IStatement
{
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly IStatement indexatorDeclaration = indexatorDeclaration;
    private readonly IExpression condition = condition;
    private readonly IStatement iterator = iterator;
    private readonly IStatement block = block;

    public void Execute()
    {
        indexatorDeclaration.Execute();

        IValue conditionVal = condition.Evaluate();
        if (conditionVal is not BoolValue bv) throw new Exception($"Условное выражение должно иметь тип boolean. Выражение имеет тип {conditionVal.Type}");

        while (IsConditionTrue(condition.Evaluate()))
        {
            variableStorage.EnterScope();
            try
            {
                try { block.Execute(); iterator.Execute(); }
                catch (BreakStatement) { break; }
                catch (ContinueStatement) { iterator.Execute(); continue; }
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
