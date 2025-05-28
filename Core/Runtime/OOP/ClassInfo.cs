using Core.Values;

namespace Core.Runtime.OOP;

public class ClassInfo(string name)
{
    public string Name { get; } = name;
    public Dictionary<string, FieldInfo> Fields { get; } = [];

    public void AddField(string name, FieldInfo field)
    {
        if (Fields.ContainsKey(name)) throw new Exception($"Объявление невозможно: поле с именем '{name}' уже существует.");

        Fields.Add(name, field);
    }
}

public class FieldInfo(TypeValue type, AccessModifier access, IValue value)
{
    public TypeValue Type { get; } = type;
    public AccessModifier Access { get; } = access;
    public IValue Value { get; set; } = value;
}

public enum AccessModifier
{
    Public, Private
}
