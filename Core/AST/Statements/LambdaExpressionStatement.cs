using Core.Expressions;

namespace Core.AST.Statements;

public class LambdaExpressionStatement(List<(string, string)>? parameters, IExpression expression) : IStatement
{
    public List<(string, string)>? Parameters { get; } = parameters;
    public IExpression Expression { get; } = expression;
}
