using Core.Values;

namespace Core.Runtime.OOP;

public class ClassInstance(ClassInfo classInfo)
{
    public ClassInfo ClassInfo { get; } = classInfo;

    /*public IValue GetFieldValue(string name, bool isThisContext = false)
    {
        if (!classInfo.Fields.TryGetValue(name, out FieldInfo? fieldInfo)) throw new Exception($"Невозможно получить значение поля: поле с именем '{name}' в классе '{classInfo.Name}' не объявлено.");
        if (!isThisContext && fieldInfo.Access == AccessModifier.Private) throw new Exception($"Невозможно получить значение поля: поле с именем '{name}' в классе '{classInfo.Name}' помечено как защищенное.");

        if (fieldInfo.IsStatic && !isThisContext) throw new Exception($"Поле '{name}' является статическим и должно быть доступно через имя класса.");

        if (!fieldInfo.IsStatic && isThisContext && ClassInfo.CurrentMethod?.IsStatic == true) throw new Exception($"В статическом методе '{ClassInfo.CurrentMethod.Name}' нельзя обращаться к нестатическому полю '{name}' через this.");

        return fieldInfo.Value;
    }

    public void SetFieldValue(string name, IValue value, bool isThisContext = false)
    {
        if (!classInfo.Fields.TryGetValue(name, out FieldInfo? fieldInfo)) throw new Exception($"Невозможно присвоить новое значение полю: поле с именем '{name}' в классе '{classInfo.Name}' не объявлено.");
        if (!isThisContext && fieldInfo.Access == AccessModifier.Private) throw new Exception($"Невозможно присвоить новое значение полю: поле с именем '{name}' в классе '{classInfo.Name}' помечено как защищенное.");
        if (fieldInfo.Type != value.Type) throw new Exception($"Невозможно присвоить новое значение полю: несоответствие типов ('{fieldInfo.Type}' и '{value.Type}').");

        if (fieldInfo.IsStatic && !isThisContext) throw new Exception($"Поле '{name}' является статическим и должно быть доступно через имя класса.");

        if (!fieldInfo.IsStatic && isThisContext && ClassInfo.CurrentMethod?.IsStatic == true) throw new Exception($"В статическом методе '{ClassInfo.CurrentMethod.Name}' нельзя обращаться к нестатическому полю '{name}' через this.");

        fieldInfo.Value = value;
    }

    public IValue CallMethod(string name, IValue[] args, bool isThisContext = false)
    {
        if (!ClassInfo.Methods.TryGetValue(name, out var methodInfo)) throw new Exception($"Метод '{name}' не найден в классе '{ClassInfo.Name}'.");

        if (!isThisContext && methodInfo.Access == AccessModifier.Private) throw new Exception($"Невозможно выполнить вызов метода: метод с именем '{name}' в классе '{classInfo.Name}' помечен как защищенный.");

        if (methodInfo.IsStatic && !isThisContext) throw new Exception($"Метод '{name}' является статическим и должен вызываться через имя класса.");

        if (!methodInfo.IsStatic && isThisContext && ClassInfo.CurrentMethod?.IsStatic == true) throw new Exception($"В статическом методе '{ClassInfo.CurrentMethod.Name}' нельзя вызывать нестатический метод '{name}' через this.");

        var previousMethod = ClassInfo.CurrentMethod;
        ClassInfo.CurrentMethod = methodInfo.UserFunction;

        try { return methodInfo.UserFunction.Execute(args); }
        finally {  ClassInfo.CurrentMethod = previousMethod; }
    }

    public MethodInfo GetMethod(string name, bool isThisContext = false)
    {
        if (!ClassInfo.Methods.TryGetValue(name, out MethodInfo? methodInfo)) throw new Exception($"Метод '{name}' не найден в классе '{ClassInfo.Name}'.");

        if (methodInfo.Access == AccessModifier.Private && !isThisContext) throw new Exception($"Метод '{name}' является приватным и недоступен извне класса '{ClassInfo.Name}'.");

        return methodInfo;
    }*/
}
