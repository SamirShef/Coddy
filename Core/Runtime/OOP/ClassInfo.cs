using Core.Runtime.Functions;
using Core.Values;

namespace Core.Runtime.OOP;

public class ClassInfo(string name)
{
    public string Name { get; } = name;
    public Dictionary<string, FieldInfo> Fields { get; } = [];
    public Dictionary<string, MethodInfo> Methods { get; } = [];
    public UserFunction? Constructor { get; private set; }
    public bool IsStatic { get; set; }
    public UserFunction? CurrentMethod { get; set; }

    public void AddField(string name, FieldInfo field)
    {
        if (Fields.ContainsKey(name)) throw new Exception($"Объявление невозможно: поле с именем '{name}' уже существует.");

        Fields.Add(name, field);
    }

    public void AddMethod(string name, MethodInfo method)
    {
        if (Methods.ContainsKey(name)) throw new Exception($"Объявление невозможно: метод с именем '{name}' уже существует.");
        if (IsStatic && !method.IsStatic) throw new Exception($"Объявление невозможно: в статическом классе все методы должны быть статическими.");

        Methods.Add(name, method);
    }

    public void SetConstructor(UserFunction constructor)
    {
        if (Constructor != null) throw new Exception($"Объявление невозможно: конструктор уже существует.");
        if (IsStatic) throw new Exception($"Объявление невозможно: статический класс не может иметь конструктор.");

        Constructor = constructor;
    }
}

public class FieldInfo(TypeValue type, AccessModifier access, IValue value, bool isStatic = false)
{
    public TypeValue Type { get; } = type;
    public AccessModifier Access { get; } = access;
    public IValue Value { get; set; } = value;
    public bool IsStatic { get; } = isStatic;
}

public class MethodInfo(AccessModifier access, UserFunction userFunction, bool isStatic = false)
{
    public AccessModifier Access { get; } = access;
    public UserFunction UserFunction { get; } = userFunction;
    public bool IsStatic { get; } = isStatic;
}

public enum AccessModifier
{
    Public, Private
}
