using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class ClassDeclarationStatement(ClassStorage classStorage, ClassInfo classInfo, List<IStatement> statements) : IStatement
{
    public void Execute()
    {
        foreach (IStatement statement in statements) statement.Execute();

        classStorage.Declare(classInfo.Name, classInfo);
    }
}
