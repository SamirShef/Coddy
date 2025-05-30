using Core.Runtime.Functions;
using Core.Values;

namespace Core.Runtime.OOP;

public class ClassInstance
{
    private readonly ClassInfo classInfo;
    private readonly Dictionary<string, IValue> fields = [];
    private readonly Dictionary<string, UserFunction> methods = [];

    public ClassInstance(ClassInfo classInfo)
    {
        this.classInfo = classInfo;

        foreach (var field in classInfo.Fields) fields.Add(field.Key, field.Value.Value);
        foreach (var method in classInfo.Methods) methods.Add(method.Key, method.Value.UserFunction);
    }

    public IValue GetFieldValue(string name, bool isThisContext = false)
    {
        if (!classInfo.Fields.ContainsKey(name)) throw new Exception($"Невозможно получить значение поля: поле с именем '{name}' в классе '{classInfo.Name}' не объявлено.");
        if (!isThisContext && classInfo.Fields[name].Access == AccessModifier.Private) throw new Exception($"Невозможно получить значение поля: поле с именем '{name}' в классе '{classInfo.Name}' помечено как защищенное.");

        return fields[name]; 
    }

    public void SetFieldValue(string name, IValue value, bool isThisContext = false)
    {
        if (!classInfo.Fields.ContainsKey(name)) throw new Exception($"Невозможно присвоить новое значение полю: поле с именем '{name}' в классе '{classInfo.Name}' не объявлено.");
        if (!isThisContext && classInfo.Fields[name].Access == AccessModifier.Private) throw new Exception($"Невозможно присвоить новое значение полю: поле с именем '{name}' в классе '{classInfo.Name}' помечено как защищенное.");
        if (classInfo.Fields[name].Type != value.Type) throw new Exception($"Невозможно присвоить новое значение полю: несоответствие типов ('{classInfo.Fields[name].Type}' и '{value.Type}').");

        fields[name] = value;
    }

    public IValue CallMethod(string name, IValue[] args, bool isThisContext = false)
    {
        if (!classInfo.Methods.ContainsKey(name)) throw new Exception($"Невозможно выполнить вызов метода: метод с именем '{name}' в классе '{classInfo.Name}' не объявлен.");
        if (!isThisContext && classInfo.Methods[name].Access == AccessModifier.Private) throw new Exception($"Невозможно выполнить вызов метода: метод с именем '{name}' в классе '{classInfo.Name}' помечен как защищенный.");

        return methods[name].Execute(args);
    }
}
