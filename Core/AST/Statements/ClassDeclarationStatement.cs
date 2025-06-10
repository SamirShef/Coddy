using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class ClassDeclarationStatement(ClassInfo classInfo, List<IStatement> statements) : IStatement
{
    public ClassInfo ClassInfo { get; } = classInfo;
    public List<IStatement> Statements = statements;
}
