using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class ClassDeclarationStatement(ClassStorage classStorage, ClassInfo classInfo, List<IStatement> statements) : IStatement
{
    public ClassInfo ClassInfo { get; } = classInfo;
    public List<IStatement> Statements = statements;

    public void Execute()
    {
        foreach (IStatement statement in Statements) statement.Execute();

        classStorage.Declare(ClassInfo.Name, ClassInfo);
    }
}
