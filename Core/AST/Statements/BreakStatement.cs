namespace Core.AST.Statements;

public class BreakStatement : Exception, IStatement { public void Execute() => throw this; }
