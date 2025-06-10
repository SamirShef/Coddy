using Core.Expressions;
using Core.Lexer;

namespace Core.AST.Statements;

public class FieldAssignmentStatement(IExpression start, IExpression targetExpression, string name, Token opToken, IExpression expression) : IStatement
{
    public IExpression Start { get; } = start;
    public IExpression TargetExpression { get; } = targetExpression;
    public string Name { get; } = name;
    public Token OpToken { get; } = opToken;
    public IExpression Expression { get; } = expression;
}
