using Core.Expressions;
using Core.Runtime.OOP;
using Core.Values;

namespace Core.AST.Statements;

public class FieldAssignmentStatement(IExpression targetExpression, string name, IExpression expression) : IStatement
{
    public void Execute()
    {
        IValue targetValue = targetExpression.Evaluate();
        if (targetValue is not ClassInstance instance) throw new Exception($"Невозможно присвоить новое значение полю: целевой объект не является классом.");

        instance.SetFieldValue(name, expression.Evaluate());
    }
}
