namespace Core.AST.Statements;

public class BlockStatement(List<IStatement> statements) : IStatement
{
    public List<IStatement> Statements { get; } = statements;
}
