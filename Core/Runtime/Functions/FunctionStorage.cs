using Core.Values;

namespace Core.Runtime.Functions;

public class FunctionStorage
{
    private readonly Dictionary<string, IFunction> functions = new()
    {
        { "println", new PrintFunction() },
        { "input", new InputFunction() },
    };

    public void Declare(string name, IFunction function)
    {
        if (functions.ContainsKey(name)) throw new Exception($"Объявление невозможно: функция/метод '{name}' уже объявлен в текущем контексте.");

        functions.Add(name, function);
    }

    public IFunction Get(string name)
    {
        if (!functions.ContainsKey(name)) throw new Exception($"Не удается вызвать функцию/метод с именем '{name}'. Функция/метод не объявлен.");

        return functions[name];
    }
}

public class PrintFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.Void;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'println()' ожидала 1 аргумент, а получила {args.Length}.");
        Console.WriteLine(args[0].AsString());
        return new VoidValue();
    }
}

public class InputFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.String;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 0) throw new Exception($"Функция 'input()' ожидала 0 аргументов, а получила {args.Length}.");
        return new StringValue(Console.ReadLine() ?? "");
    }
}
