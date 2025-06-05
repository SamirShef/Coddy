namespace Core.AST.Statements;

public class ContinueStatement : Exception, IStatement { public void Execute() => throw this; }
