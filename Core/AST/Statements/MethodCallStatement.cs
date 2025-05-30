using Core.Expressions;

namespace Core.AST.Statements;

public class MethodCallStatement(IExpression expression) : IStatement { public void Execute() => expression.Evaluate(); }