using Core.Expressions;

namespace Core.AST.Statements;

public class IfElseStatement(IExpression condition, IStatement ifBlock, IStatement? elseBlock) : IStatement
{
    public IExpression Condition { get; } = condition;
    public IStatement IfBlock { get; } = ifBlock;
    public IStatement? ElseBlock { get; } = elseBlock;
}
