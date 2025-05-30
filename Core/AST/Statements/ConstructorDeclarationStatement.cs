using Core.Runtime.Functions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class ConstructorDeclarationStatement(ClassInfo classInfo, UserFunction constructor) : IStatement { public void Execute() => classInfo.SetConstructor(constructor); } 