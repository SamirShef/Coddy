
using Core.Values;

namespace Core.Runtime.OOP;

public class ClassInstance(ClassInfo classInfo)
{
    private readonly ClassInfo classInfo = classInfo;

    public IValue GetFieldValue(string name, bool isThisContext = false)
    {
        if (!classInfo.Fields.ContainsKey(name)) throw new Exception($"Невозможно получить значение поля: поле с именем '{name}' в классе '{classInfo.Name}' не объявлено.");
        if (!isThisContext && classInfo.Fields[name].Access == AccessModifier.Private) throw new Exception($"Невозможно получить значение поля: поле с именем '{name}' в классе '{classInfo.Name}' помечено как защищенное.");

        return classInfo.Fields[name].Value; 
    }

    public void SetFieldValue(string name, IValue value, bool isThisContext = false)
    {
        if (!classInfo.Fields.ContainsKey(name)) throw new Exception($"Невозможно присвоить новое значение полю: поле с именем '{name}' в классе '{classInfo.Name}' не объявлено.");
        if (!isThisContext && classInfo.Fields[name].Access == AccessModifier.Private) throw new Exception($"Невозможно присвоить новое значение полю: поле с именем '{name}' в классе '{classInfo.Name}' помечено как защищенное.");
        if (classInfo.Fields[name].Type != value.Type) throw new Exception($"Невозможно присвоить новое значение полю: несоответствие типов ('{classInfo.Fields[name].Type}' и '{value.Type}').");

        classInfo.Fields[name].Value = value;
    }

    public IValue CallMethod(string name, IValue[] args, bool isThisContext = false)
    {
        if (!classInfo.Methods.ContainsKey(name)) throw new Exception($"Невозможно выполнить вызов метода: метод с именем '{name}' в классе '{classInfo.Name}' не объявлен.");
        if (!isThisContext && classInfo.Methods[name].Access == AccessModifier.Private) throw new Exception($"Невозможно выполнить вызов метода: метод с именем '{name}' в классе '{classInfo.Name}' помечен как защищенный.");

        return classInfo.Methods[name].UserFunction.Execute(args);
    }

    public MethodInfo GetMethod(string name, bool isThisContext = false)
    {
        if (!classInfo.Methods.TryGetValue(name, out MethodInfo? methodInfo)) throw new Exception($"Метод '{name}' не найден в классе '{classInfo.Name}'.");

        if (methodInfo.Access == AccessModifier.Private && !isThisContext) throw new Exception($"Метод '{name}' является приватным и недоступен извне класса '{classInfo.Name}'.");

        return methodInfo;
    }
}
