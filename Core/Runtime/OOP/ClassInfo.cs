using Core.Runtime.Functions;
using Core.Values;

namespace Core.Runtime.OOP;

public class ClassInfo(string name)
{
    public string Name { get; } = name;
    public Dictionary<string, FieldInfo> Fields { get; } = [];
    public Dictionary<string, MethodInfo> Methods { get; } = [];

    public void AddField(string name, FieldInfo field)
    {
        if (Fields.ContainsKey(name)) throw new Exception($"Объявление невозможно: поле с именем '{name}' уже существует.");

        Fields.Add(name, field);
    }

    public void AddMethod(string name, MethodInfo method)
    {
        if (Methods.ContainsKey(name)) throw new Exception($"Объявление невозможно: метод с именем '{name}' уже существует.");

        Methods.Add(name, method);
    }
}

public class FieldInfo(TypeValue type, AccessModifier access, IValue value)
{
    public TypeValue Type { get; } = type;
    public AccessModifier Access { get; } = access;
    public IValue Value { get; set; } = value;
}

public class MethodInfo(AccessModifier access, UserFunction userFunction)
{
    public AccessModifier Access { get; } = access;
    public UserFunction UserFunction { get; } = userFunction;
}

public enum AccessModifier
{
    Public, Private
}
