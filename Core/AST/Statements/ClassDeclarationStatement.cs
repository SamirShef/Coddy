using Core.Runtime;

namespace Core.AST.Statements;

public class ClassDeclarationStatement(ClassStorage classStorage, string name, List<FieldDeclarationStatement> fields, List<MethodDeclarationStatement> methods) : IStatement
{
    public void Execute()
    {
        var classInfo = new ClassInfo(name);

        classStorage.Declare(classInfo);

        foreach (var field in fields) field.Execute();

        foreach (var method in methods) method.Execute();
    }
}
