namespace Core.Values;

public class BoolValue(bool value) : IValue
{
    public object Value { get; set; } = value;
    public TypeValue Type => TypeValue.Bool;

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
        if (other is BoolValue bv) return new BoolValue(AsBool() == GetOtherValue(bv));

        throw new Exception($"Невозможно применить оператор '==' с типом {Type} и {other.Type}.");
    }

    public IValue NotEquals(IValue other)
    {
        if (other is BoolValue bv) return new BoolValue(AsBool() != GetOtherValue(bv));

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
        if (other is BoolValue bv) return new BoolValue(AsBool() && GetOtherValue(bv));

        throw new Exception($"Невозможно применить оператор '&&' с типом {Type} и {other.Type}.");
    }

    public IValue Or(IValue other)
    {
        if (other is BoolValue bv) return new BoolValue(AsBool() || GetOtherValue(bv));

        throw new Exception($"Невозможно применить оператор '||' с типом {Type} и {other.Type}.");
    }

    public string AsString() => Value.ToString();

    public bool AsBool() => (bool)Value;
    private bool GetOtherValue(BoolValue other)
    {
        if (other is BoolValue bv) return bv.AsBool();

        throw new Exception($"Несоответствие типов (тип {Type} и {other.Type}).");
    }
}
