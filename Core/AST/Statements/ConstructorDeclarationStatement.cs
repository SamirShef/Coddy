using Core.Runtime.Functions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class ConstructorDeclarationStatement(ClassInfo classInfo, UserFunction constructor) : IStatement
{
    public ClassInfo ClassInfo { get; } = classInfo;
    public UserFunction Constructor { get; } = constructor;

    public void Execute() => ClassInfo.SetConstructor(Constructor);
} 