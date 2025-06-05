namespace Core.AST.Statements;

public class BlockStatement(List<IStatement> statements) : IStatement
{
    public List<IStatement> Statements { get; } = statements;

    public void Execute()
    {
        foreach (IStatement statement in Statements) statement.Execute();
    }
}
