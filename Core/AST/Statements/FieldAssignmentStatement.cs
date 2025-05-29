using Core.Expressions;
using Core.Values;

namespace Core.AST.Statements;

public class FieldAssignmentStatement(IExpression targetExpression, string name, IExpression expression) : IStatement
{
    public void Execute()
    {
        IValue targetValue = targetExpression.Evaluate();
        if (targetValue is not ClassValue cv) throw new Exception($"Невозможно присвоить новое значение полю: целевой объект не является классом.");

        cv.Instance.SetFieldValue(name, expression.Evaluate());
    }
}
