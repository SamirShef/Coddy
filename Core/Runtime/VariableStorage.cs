using Core.Values;

namespace Core.Runtime;

public class VariableStorage
{
    private readonly Stack<Dictionary<string, VariableInfo>> scopes = [];

    public VariableStorage() => EnterScope();

    public void EnterScope() => scopes.Push([]);
    public void ExitScope()
    {
        if (scopes.Count == 0) throw new Exception("Невозможно выйти за пределы глобальной области видимости.");

        scopes.Pop();
    }

    public void Declare(string name, TypeValue type, IValue? value)
    {
        var scope = scopes.Peek();

        if (scope.ContainsKey(name)) throw new Exception($"Объявление переменной невозможно: переменная '{name}' уже объявлена в текущем контексте.");
        scope.Add(name, new VariableInfo(type, value ?? Helpers.GetDefaultValue(type)));
    }

    public VariableInfo Get(string name)
    {
        foreach (var scope in scopes) if (scope.TryGetValue(name, out VariableInfo? variableInfo)) return variableInfo;

        throw new Exception($"Не удается получить значение переменной с именем '{name}'. Переменная не объявлена.");
    }

    public void Set(string name, TypeValue type, IValue value)
    {
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey(name))
            {
                scope[name] = new VariableInfo(type, value);
                return;
            }
        }

        throw new Exception($"Не удается установить значение для переменной с именем '{name}'. Переменная не объявлена.");
    }

    public bool Exist(string name)
    {
        foreach (var scope in scopes) if (scope.ContainsKey(name)) return true;
        return false;
    }
}

public class VariableInfo (TypeValue type, IValue value)
{
    public TypeValue Type { get; set; } = type;
    public IValue Value { get; set; } = value;
}