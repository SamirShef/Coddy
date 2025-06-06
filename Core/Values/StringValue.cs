namespace Core.Values;

public class StringValue(string value) : IValue
{
    public object Value { get; set; } = value;
    public TypeValue Type => TypeValue.String;

    public IValue Add(IValue other)
    {
        if (other is StringValue sv) return new StringValue(AsString() + GetOtherValue(sv));

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
        if (other is StringValue sv) return new BoolValue(AsString() == GetOtherValue(sv));

        throw new Exception($"Невозможно применить оператор '==' с типом {Type} и {other.Type}.");
    }

    public IValue NotEquals(IValue other)
    {
        if (other is StringValue sv) return new BoolValue(AsString() != GetOtherValue(sv));

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

    private string GetOtherValue(StringValue other)
    {
        if (other is StringValue sv) return sv.AsString();

        throw new Exception($"Несоответствие типов (тип {Type} и {other.Type}).");
    }
}
