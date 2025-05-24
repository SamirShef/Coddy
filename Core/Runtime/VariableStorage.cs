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

        if (scope.ContainsKey(name)) throw new Exception($"Объявление невозможно: переменная/поле '{name}' уже объявлено в текущем контексте.");
        scope.Add(name, new VariableInfo(type, value ?? GetDefaultValue(type)));
    }

    public VariableInfo Get(string name)
    {
        foreach (var scope in scopes) if (scope.TryGetValue(name, out VariableInfo? variableInfo)) return variableInfo;

        throw new Exception($"Не удается получить значение переменной/поля с именем '{name}'. Переменная/поле не объявлено.");
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

        throw new Exception($"Не удается установить значение для переменной/поля с именем '{name}'. Переменная не объявлена.");
    }

    public bool Exist(string name)
    {
        foreach (var scope in scopes) if (scope.ContainsKey(name)) return true;
        return false;
    }

    private IValue GetDefaultValue(TypeValue type) => type switch
    {
        TypeValue.Int => new IntValue(0),
        TypeValue.Float => new FloatValue(0f),
        TypeValue.Double => new DoubleValue(0d),
        TypeValue.Decimal => new DecimalValue(0m),
        TypeValue.String => new StringValue(""),
        TypeValue.Bool => new BoolValue(false),
        _ => throw new Exception($"Не удается получить стандартное значение для типа переменной '{type}'.")
    };
}

public class VariableInfo (TypeValue type, IValue value)
{
    public TypeValue Type { get; set; } = type;
    public IValue Value { get; set; } = value;
}