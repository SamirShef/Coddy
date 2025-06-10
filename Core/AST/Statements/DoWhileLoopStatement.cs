using Core.Expressions;

namespace Core.AST.Statements;

public class DoWhileLoopStatement(IExpression condition, IStatement block) : IStatement
{
    public IExpression Condition { get; } = condition;
    public IStatement Block { get; } = block;
}
