using Core.Expressions;
using Core.Runtime.Functions;
using Core.Runtime.OOP;

namespace Core.AST.Statements;

public class ConstructorDeclarationStatement(ClassInfo classInfo, UserFunction constructor, List<IExpression> parentParameters) : IStatement
{
    public ClassInfo ClassInfo { get; } = classInfo;
    public UserFunction Constructor { get; } = constructor;
    public List<IExpression> ParentParameters { get; } = parentParameters;
} 