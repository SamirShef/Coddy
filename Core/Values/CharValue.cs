namespace Core.Values;

public class CharValue(string value) : IValue
{
    public object Value { get; set; } = value;
    public TypeValue Type => TypeValue.Char;

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
        if (other is CharValue cv) return new BoolValue(AsRawString() == cv.AsRawString());

        throw new Exception($"Невозможно применить оператор '==' с типом {Type} и {other.Type}.");
    }

    public IValue NotEquals(IValue other)
    {
        if (other is CharValue cv) return new BoolValue(AsRawString() != cv.AsRawString());

        throw new Exception($"Невозможно применить оператор '!=' с типом {Type} и {other.Type}.");
    }

    public IValue Greater(IValue other)
    {
        if (other is CharValue cv) return new BoolValue(AsRawString().CompareTo(cv.AsRawString()) > 0);

        throw new Exception($"Невозможно применить оператор '>' с типом {Type} и {other.Type}.");
    }

    public IValue GreaterEqual(IValue other)
    {
        if (other is CharValue cv) return new BoolValue(AsRawString().CompareTo(cv.AsRawString()) >= 0);

        throw new Exception($"Невозможно применить оператор '>=' с типом {Type} и {other.Type}.");
    }

    public IValue Less(IValue other)
    {
        if (other is CharValue cv) return new BoolValue(AsRawString().CompareTo(cv.AsRawString()) < 0);

        throw new Exception($"Невозможно применить оператор '<' с типом {Type} и {other.Type}.");
    }

    public IValue LessEqual(IValue other)
    {
        if (other is CharValue cv) return new BoolValue(AsRawString().CompareTo(cv.AsRawString()) <= 0);

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

    public string AsString() => (string)Value;
    public string AsRawString() => (string)Value;
    public char AsChar()
    {
        if (AsString().Length == 1) return AsString()[0];

        throw new Exception("Невозможно преобразовать escape-последовательность в char без интерпретации.");
    }
}
