namespace Core.Values;

public class DoubleValue(double value) : IValue
{
    public object Value { get; set; } = value;
    public TypeValue Type => TypeValue.Float;

    public IValue Add(IValue other)
    {
        if (other is DoubleValue dv) return new DoubleValue(AsDouble() + GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '+' с типом {Type} и {other.Type}.");
    }

    public IValue Subtract(IValue other)
    {
        if (other is DoubleValue dv) return new DoubleValue(AsDouble() - GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '-' с типом {Type} и {other.Type}.");
    }

    public IValue Multiply(IValue other)
    {
        if (other is DoubleValue dv) return new DoubleValue(AsDouble() * GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '*' с типом {Type} и {other.Type}.");
    }

    public IValue Divide(IValue other)
    {
        if (other is DoubleValue dv)
        {
            if (dv.AsDouble() == 0) throw new Exception("Данная операция возвращает деление на ноль.");
            return new DoubleValue(AsDouble() / GetOtherValue(dv));
        }

        throw new Exception($"Невозможно применить оператор '/' с типом {Type} и {other.Type}.");
    }

    public IValue Modulo(IValue other)
    {
        if (other is DoubleValue dv) return new DoubleValue(AsDouble() % GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '%' с типом {Type} и {other.Type}.");
    }

    public IValue Equals(IValue other)
    {
        if (other is DoubleValue dv) return new BoolValue(AsDouble() == GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '==' с типом {Type} и {other.Type}.");
    }

    public IValue NotEquals(IValue other)
    {
        if (other is DoubleValue dv) return new BoolValue(AsDouble() != GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '!=' с типом {Type} и {other.Type}.");
    }

    public IValue Greater(IValue other)
    {
        if (other is DoubleValue dv) return new BoolValue(AsDouble() > GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '>' с типом {Type} и {other.Type}.");
    }

    public IValue GreaterEqual(IValue other)
    {
        if (other is DoubleValue dv) return new BoolValue(AsDouble() >= GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '>=' с типом {Type} и {other.Type}.");
    }

    public IValue Less(IValue other)
    {
        if (other is DoubleValue dv) return new BoolValue(AsDouble() < GetOtherValue(dv));

        throw new Exception($"Невозможно применить оператор '<' с типом {Type} и {other.Type}.");
    }

    public IValue LessEqual(IValue other)
    {
        if (other is DoubleValue dv) return new BoolValue(AsDouble() <= GetOtherValue(dv));

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

    private double AsDouble() => (float)Value;
    private double GetOtherValue(DoubleValue other)
    {
        if (other is DoubleValue dv) return dv.AsDouble();

        throw new Exception($"Несоответствие типов (тип {Type} и {other.Type}).");
    }
}
