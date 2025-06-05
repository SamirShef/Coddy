namespace Core.Runtime.OOP;

public class ClassStorage
{
    private readonly Dictionary<string, ClassInfo> classes = [];

    public void Declare(string name, ClassInfo classInfo)
    {
        if (classes.ContainsKey(name)) throw new Exception($"Декларация класса невозможна: класс с именем '{name}' уже существует.");
        classes.Add(name, classInfo);
    }

    public ClassInfo Get(string name)
    {
        if (!classes.ContainsKey(name)) throw new Exception($"Класс с именем '{name}' не объявлен.");

        return classes[name];
    }

    public bool Exist(string name) => classes.ContainsKey(name);
}
