using Core.Values;

namespace Core.Runtime.Functions;

public class FunctionStorage
{
    private readonly Dictionary<string, IFunction> functions = new()
    {
        { "println", new PrintFunction() },
        { "input", new InputFunction() },
        { "to_int", new ToIntFunction() },
        { "to_float", new ToFloatFunction() },
        { "to_double", new ToDoubleFunction() },
        { "to_decimal", new ToDecimalFunction() },
        { "to_string", new ToStringFunction() },
        { "to_boolean", new ToBooleanFunction() },
        { "len", new LenFunction() },
        { "type", new TypeFunction() },
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

public class ToIntFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.Int;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'int()' ожидала 1 аргумент, а получила {args.Length}.");
        if (args[0] is not StringValue sv) throw new Exception($"Функция 'int()' ожидала тип аргумента {args[0].Type}, а получила String.");
        return new IntValue(int.Parse(sv.AsString()));
    }
}

public class ToFloatFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.Float;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'float()' ожидала 1 аргумент, а получила {args.Length}.");
        if (args[0] is not StringValue sv) throw new Exception($"Функция 'float()' ожидала тип аргумента {args[0].Type}, а получила String.");
        return new FloatValue(float.Parse(sv.AsString().Replace(".", ",")));
    }
}

public class ToDoubleFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.Double;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'double()' ожидала 1 аргумент, а получила {args.Length}.");
        if (args[0] is not StringValue sv) throw new Exception($"Функция 'double()' ожидала тип аргумента {args[0].Type}, а получила String.");
        return new DoubleValue(double.Parse(sv.AsString().Replace(".", ",")));
    }
}

public class ToDecimalFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.Decimal;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'decimal()' ожидала 1 аргумент, а получила {args.Length}.");
        if (args[0] is not StringValue sv) throw new Exception($"Функция 'decimal()' ожидала тип аргумента {args[0].Type}, а получила String.");
        return new DecimalValue(decimal.Parse(sv.AsString().Replace(".", ",")));
    }
}

public class ToStringFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.String;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'string()' ожидала 1 аргумент, а получила {args.Length}.");
        return new StringValue(args[0].AsString());
    }
}

public class ToBooleanFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.Bool;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'boolean()' ожидала 1 аргумент, а получила {args.Length}.");
        if (args[0] is not StringValue sv) throw new Exception($"Функция 'boolean()' ожидала тип аргумента {args[0].Type}, а получила String.");
        if (sv.AsString() != "false" || sv.AsString() != "true") throw new Exception($"Строковый литерал '{sv.AsString()}' невозможно преобразовать в тип boolean");
        return new BoolValue(bool.Parse(sv.AsString()));
    }
}

public class LenFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.String;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'type()' ожидала 1 аргумент, а получила {args.Length}.");
        if (args[0] is StringValue sv) return new IntValue(sv.AsString().Length);
        if (args[0] is ArrayValue av) return new IntValue(av.AsArray().Length);

        throw new Exception($"Функция 'len()' ожидала тип аргумента string или array, а получила {args[0].GetType().Name.ToLower()}.");
    }
}

public class TypeFunction : IFunction
{
    public TypeValue ReturnType => TypeValue.String;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != 1) throw new Exception($"Функция 'type()' ожидала 1 аргумент, а получила {args.Length}.");
        return new StringValue(args[0].Type.ToString());
    }
}