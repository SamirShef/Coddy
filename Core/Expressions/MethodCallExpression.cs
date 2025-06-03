using Core.Runtime.OOP;
using Core.Values;

namespace Core.Expressions;

public class MethodCallExpression(IExpression target, string methodName, List<IExpression> arguments, ClassStorage? classStorage = null, IExpression? start = null) : IExpression
{
    private readonly IExpression target = target;
    private readonly string methodName = methodName;
    private readonly List<IExpression> arguments = arguments;

    public IExpression Target { get; } = target;
    public string MethodName { get; } = methodName;
    public List<IExpression> Args { get; } = arguments;

    public IValue Evaluate()
    {
        if (start != null && classStorage != null && start is VariableExpression startVariable && classStorage.Exist(startVariable.Name))
            return new StaticMethodCallExpression(classStorage, startVariable.Name, MethodName, Args).Evaluate();

        IValue targetValue = target.Evaluate();
        
        if (targetValue is not ClassValue cv) throw new Exception($"Невозможно вызывать метод: тип {targetValue.Type} не является объектом.");

        bool isThisContext = target is VariableExpression ve && ve.Name == "this";
        bool isStaticContext = isThisContext && cv.Instance.ClassInfo.Methods.TryGetValue(methodName, out var methodInfo) && methodInfo.IsStatic;
        
        if (isStaticContext)
        {
            // В статическом контексте разрешаем доступ только к статическим полям
            foreach (var arg in arguments)
            {
                if (arg is FieldExpression fieldExpr && fieldExpr.Target is VariableExpression fieldTarget && fieldTarget.Name == "this")
                {
                    if (!cv.Instance.ClassInfo.Fields.TryGetValue(fieldExpr.Name, out var fieldInfo) || !fieldInfo.IsStatic)
                        throw new Exception($"В статическом методе '{methodName}' нельзя обращаться к нестатическому полю '{fieldExpr.Name}' через this.");
                }
            }
        }

        IValue result = cv.Instance.CallMethod(methodName, [.. arguments.Select(arg => arg.Evaluate())], isThisContext);
        
        if (result is ClassValue resultClass) return resultClass;
        
        if (result is VoidValue) return targetValue;

        return result;
    }
}