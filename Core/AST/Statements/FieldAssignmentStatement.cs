using Core.Expressions;
using Core.Lexer;
using Core.Runtime.OOP;
using Core.Values;

namespace Core.AST.Statements;

public class FieldAssignmentStatement(IExpression start, IExpression targetExpression, ClassStorage classStorage, string name, Token opToken, IExpression expression) : IStatement
{
    public IExpression Start { get; } = start;
    public IExpression TargetExpression { get; } = targetExpression;
    public string Name { get; } = name;
    public Token OpToken { get; } = opToken;
    public IExpression Expression { get; } = expression;

    public void Execute()
    {
        if (Start is VariableExpression startVariable && classStorage.Exist(startVariable.Name))
        {
            classStorage.Get(startVariable.Name).Fields[Name].Value = Expression.Evaluate();
        }
        else
        {
            IValue targetValue = TargetExpression.Evaluate();
            if (targetValue is not ClassValue cv) throw new Exception($"Невозможно присвоить новое значение полю: целевой объект не является классом.");

            bool isThisContext = TargetExpression is VariableExpression ve && ve.Name == "this";
            cv.Instance.SetFieldValue(Name, Expression.Evaluate(), isThisContext);
        }
    }
}
