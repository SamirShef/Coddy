using Core.Expressions;

namespace Core.AST.Statements;

public class WhileLoopStatement(IExpression condition, IStatement block) : IStatement
{
    public IExpression Condition { get; } = condition;
    public IStatement Block { get; } = block;
}
