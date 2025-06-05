using Core.Runtime.OOP;

namespace Core.Values;

public class ClassValue(ClassInstance instance) : IValue
{
    public object Value { get; set; } = instance;
    public TypeValue Type => TypeValue.Class;
    public ClassInstance Instance { get; } = instance;

    public IValue Add(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '+' с типом {Type} и {other.Type}.");
    }

    public IValue Subtract(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '-' с типом {Type} и {other.Type}.");
    }

    public IValue Multiply(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '*' с типом {Type} и {other.Type}.");
    }

    public IValue Divide(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '/' с типом {Type} и {other.Type}.");
    }

    public IValue Modulo(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '%' с типом {Type} и {other.Type}.");
    }

    public IValue Equals(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '==' с типом {Type} и {other.Type}.");
    }

    public IValue NotEquals(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '!=' с типом {Type} и {other.Type}.");
    }

    public IValue Greater(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '>' с типом {Type} и {other.Type}.");
    }

    public IValue GreaterEqual(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '>=' с типом {Type} и {other.Type}.");
    }

    public IValue Less(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '<' с типом {Type} и {other.Type}.");
    }

    public IValue LessEqual(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '<=' с типом {Type} и {other.Type}.");
    }

    public IValue And(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '&&' с типом {Type} и {other.Type}.");
    }

    public IValue Or(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '||' с типом {Type} и {other.Type}.");
    }

    public string AsString() => Value.ToString();
}
