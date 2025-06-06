namespace Core.Values;

public class ArrayValue(IValue[] value, TypeValue elementsType) : IValue
{
    public object Value { get; set; } = value;
    public TypeValue Type => TypeValue.Array;
    public TypeValue ElementsType => elementsType;

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
        if (other is ArrayValue av) return new BoolValue(AsArray() == av.AsArray());

        throw new Exception($"Невозможно применить оператор '==' с типом {Type} и {other.Type}.");
    }

    public IValue NotEquals(IValue other)
    {
        if (other is ArrayValue av) return new BoolValue(AsArray() != av.AsArray());

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

    public IValue LeftShift(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '<<' с типом {Type} и {other.Type}.");
    }

    public IValue RightShift(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '>>' с типом {Type} и {other.Type}.");
    }

    public IValue LogicalRightShift(IValue other)
    {
        throw new Exception($"Невозможно применить оператор '>>>' с типом {Type} и {other.Type}.");
    }

    public string AsString() => Value.ToString();

    public IValue[] AsArray() => (IValue[])Value;
}
