using Core.Runtime.OOP;
using Core.Values;

namespace Core.Expressions;

public class NewClassExpression(ClassStorage classStorage, string name, List<IExpression>? arguments = null) : IExpression
{
    private readonly ClassStorage classStorage = classStorage;
    private readonly string name = name;
    private readonly List<IExpression>? arguments = arguments;

    public string Name { get; } = name;
    public List<IExpression>? Args { get; } = arguments;

    public IValue Evaluate()
    {
        var classInfo = classStorage.Get(name);
        var instance = new ClassInstance(classInfo);

        if (classInfo.IsStatic) throw new Exception($"Создание экземпляра класса невозможно: класс '{name}' является статическим.");
        if (classInfo.Constructor != null)
        {
            if (arguments == null) throw new Exception($"Конструктор класса '{name}' ожидает аргументы.");
            
            classInfo.Constructor.Execute([.. arguments.Select(arg => arg.Evaluate())]);
        }
        else if (arguments != null && arguments.Count > 0)
        {
            throw new Exception($"Класс '{name}' не имеет конструктора, но при создании экземпляра переданы аргументы.");
        }

        return new ClassValue(instance);
    }
}
