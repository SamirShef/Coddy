namespace Core.AST.Statements;

public class BlockStatement(List<IStatement> statements) : IStatement
{
    private List<IStatement> statements = statements;

    public void Execute()
    {
        foreach (IStatement statement in statements) statement.Execute();
    }
}
