using Core.AST.Statements;
using Core.Values;

namespace Core.Runtime;

public class ClassStorage
{
    private readonly Dictionary<string, ClassInfo> classes = [];

    public void Declare(ClassInfo classInfo)
    {
        if (classes.ContainsKey(classInfo.Name)) throw new Exception($"Объявление невозможно: класс '{classInfo.Name}' уже объявлен.");

        classes.Add(classInfo.Name, classInfo);
    }

    public ClassInfo Get(string name)
    {
        if (!classes.ContainsKey(name)) throw new Exception($"Не удается получить класс с именем '{name}'. Класс не объявлен.");

        return classes[name];
    }
}

public class ClassInfo(string name)
{
    public string Name { get; } = name;
    public Dictionary<string, FieldInfo> Fields { get; } = [];
    public Dictionary<string, MethodInfo> Methods { get; } = [];

    public void AddField(string name, FieldInfo fieldInfo) => Fields[name] = fieldInfo;

    public void AddMethod(string name, MethodInfo methodInfo) => Methods[name] = methodInfo;
}

public class FieldInfo(AccessModifier access, TypeValue type, IValue? value)
{
    public AccessModifier Access { get; } = access;
    public TypeValue Type { get; } = type;
    public IValue? Value { get; set; } = value;
}

public class MethodInfo(AccessModifier access, TypeValue returnType, List<(string, TypeValue)> parameters, IStatement body)
{
    public AccessModifier Access { get; } = access;
    public TypeValue ReturnType { get; } = returnType;
    public List<(string Name, TypeValue Type)> Parameters { get; } = parameters;
    public IStatement Body { get; } = body;
}

public enum AccessModifier
{
    Public,
    Private
}