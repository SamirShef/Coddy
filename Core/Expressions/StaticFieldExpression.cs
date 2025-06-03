using Core.Runtime.OOP;
using Core.Values;

namespace Core.Expressions;

public class StaticFieldExpression(ClassStorage classStorage, string className, string fieldName) : IExpression
{
    private readonly ClassStorage classStorage = classStorage;
    private readonly string className = className;
    private readonly string fieldName = fieldName;

    public IValue Evaluate()
    {
        ClassInfo classInfo = classStorage.Get(className);

        if (!classInfo.Fields.TryGetValue(fieldName, out FieldInfo? fieldInfo)) throw new Exception($"Поле '{fieldName}' не найдено в классе '{className}'.");

        if (!fieldInfo.IsStatic) throw new Exception($"Поле '{fieldName}' в классе '{className}' не является статическим.");

        if (fieldInfo.Access == AccessModifier.Private) throw new Exception($"Поле '{fieldName}' в классе '{className}' помечено как защищённое.");

        return fieldInfo.Value;
    }
}
