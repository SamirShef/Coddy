using Core.Runtime.OOP;
using Core.Values;

namespace Core.Expressions;

public class StaticMethodCallExpression(ClassStorage classStorage, string className, string methodName, List<IExpression> args) : IExpression
{
    private readonly ClassStorage classStorage = classStorage;
    private readonly string className = className;
    private readonly string methodName = methodName;
    private readonly List<IExpression> args = args;

    public IValue Evaluate()
    {
        ClassInfo classInfo = classStorage.Get(className);

        if (!classInfo.Methods.TryGetValue(methodName, out MethodInfo? methodInfo)) throw new Exception($"Метод '{methodName}' не найден в классе '{className}'.");

        if (!methodInfo.IsStatic) throw new Exception($"Метод '{methodName}' в классе '{className}' не является статическим.");

        if (methodInfo.Access == AccessModifier.Private) throw new Exception($"Метод '{methodName}' в классе '{className}' помечен как защищённый.");

        List<IValue> evaluatedArgs = [];
        foreach (IExpression arg in args) evaluatedArgs.Add(arg.Evaluate());

        classInfo.CurrentMethod = methodInfo.UserFunction;

        return methodInfo.UserFunction.Execute([.. evaluatedArgs]);
    }
}
