using Core.Runtime.Functions;
using Core.Runtime.OOP;
using Core.Values;

namespace Core.Expressions;

public class FieldExpression(IExpression target, string name, ClassStorage? classStorage = null, IExpression? start = null) : IExpression
{
    public IExpression Target { get; } = target;
    public string Name { get; } = name;
    public IExpression? Start { get; } = start;

    public IValue Evaluate()
    {
        if (Start != null && classStorage != null && Start is VariableExpression startVariable && classStorage.Exist(startVariable.Name)) return new StaticFieldExpression(classStorage, startVariable.Name, Name).Evaluate();

        IValue targetValue = Target.Evaluate();
        if (targetValue is not ClassValue cv) throw new Exception($"Невозможно получить значение поля: тип {targetValue.Type} не является объектом.");

        bool isThisContext = Target is VariableExpression ve && ve.Name == "this";
        
        if (isThisContext)
        {
            var currentMethod = cv.Instance.ClassInfo.CurrentMethod;
            if (currentMethod != null && currentMethod.IsStatic)
                if (!cv.Instance.ClassInfo.Fields.TryGetValue(Name, out var fieldInfo) || !fieldInfo.IsStatic)
                    throw new Exception($"В статическом методе '{currentMethod.Name}' нельзя обращаться к нестатическому полю '{Name}' через this.");
        }

        return cv.Instance.GetFieldValue(Name, isThisContext);
    }
}
