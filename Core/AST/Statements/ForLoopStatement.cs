using Core.Expressions;

namespace Core.AST.Statements;

public class ForLoopStatement(IStatement indexerDeclaration, IExpression condition, IStatement iterator, IStatement block) : IStatement
{
    public IStatement IndexerDeclaration { get; } = indexerDeclaration;
    public IExpression Condition { get; } = condition;
    public IStatement Iterator { get; } = iterator;
    public IStatement Block { get; } = block;
}
