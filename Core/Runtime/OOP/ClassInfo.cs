using Core.Runtime.Functions;

namespace Core.Runtime.OOP;

public class ClassInfo(string name)
{
    public string Name { get; } = name;
    public UserFunction? Constructor { get; private set; }
    public string? GenericsParameters { get; set; }
    public List<string> Implements { get; set; } = [];
    public bool IsStatic { get; set; }

    public void SetConstructor(UserFunction constructor)
    {
        if (Constructor != null) throw new Exception($"Объявление невозможно: конструктор уже существует.");
        if (IsStatic) throw new Exception($"Объявление невозможно: статический класс не может иметь конструктор.");

        Constructor = constructor;
    }
}

public enum AccessModifier
{
    Public, Private
}
