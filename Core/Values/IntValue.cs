namespace Core.Values;

public class IntValue(int value) : IValue
{
    public object Value { get; set; } = value;
    public TypeValue Type => TypeValue.Int;

    public IValue Add(IValue other)
    {
        if (other is IntValue iv) return new IntValue(AsInt() + GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '+' с типом {Type} и {other.Type}.");
    }

    public IValue Subtract(IValue other)
    {
        if (other is IntValue iv) return new IntValue(AsInt() - GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '-' с типом {Type} и {other.Type}.");
    }

    public IValue Multiply(IValue other)
    {
        if (other is IntValue iv) return new IntValue(AsInt() * GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '*' с типом {Type} и {other.Type}.");
    }

    public IValue Divide(IValue other)
    {
        if (other is IntValue iv)
        {
            if (iv.AsInt() == 0) throw new Exception("Данная операция возвращает деление на ноль.");
            return new IntValue(AsInt() / GetOtherValue(iv));
        }

        throw new Exception($"Невозможно применить оператор '/' с типом {Type} и {other.Type}.");
    }

    public IValue Modulo(IValue other)
    {
        if (other is IntValue iv) return new IntValue(AsInt() % GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '%' с типом {Type} и {other.Type}.");
    }

    public IValue Equals(IValue other)
    {
        if (other is IntValue iv) return new BoolValue(AsInt() == GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '==' с типом {Type} и {other.Type}.");
    }

    public IValue NotEquals(IValue other)
    {
        if (other is IntValue iv) return new BoolValue(AsInt() != GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '!=' с типом {Type} и {other.Type}.");
    }

    public IValue Greater(IValue other)
    {
        if (other is IntValue iv) return new BoolValue(AsInt() > GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '>' с типом {Type} и {other.Type}.");
    }

    public IValue GreaterEqual(IValue other)
    {
        if (other is IntValue iv) return new BoolValue(AsInt() >= GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '>=' с типом {Type} и {other.Type}.");
    }

    public IValue Less(IValue other)
    {
        if (other is IntValue iv) return new BoolValue(AsInt() < GetOtherValue(iv));

        throw new Exception($"Невозможно применить оператор '<' с типом {Type} и {other.Type}.");
    }
    
    public IValue LessEqual(IValue other)
    {
        if (other is IntValue iv) return new BoolValue(AsInt() <= GetOtherValue(iv));

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
        if (other is IntValue iv) return new IntValue(AsInt() << iv.AsInt());

        throw new Exception($"Невозможно применить оператор '<<' с типом {Type} и {other.Type}.");
    }

    public IValue RightShift(IValue other)
    {
        if (other is IntValue iv) return new IntValue(AsInt() >> iv.AsInt());

        throw new Exception($"Невозможно применить оператор '>>' с типом {Type} и {other.Type}.");
    }

    public IValue LogicalRightShift(IValue other)
    {
        if (other is IntValue iv) return new IntValue(AsInt() >>> iv.AsInt());

        throw new Exception($"Невозможно применить оператор '>>>' с типом {Type} и {other.Type}.");
    }

    public string AsString() => Value.ToString();

    public int AsInt() => (int)Value;
    private int GetOtherValue(IntValue other)
    {
        if (other is IntValue iv) return iv.AsInt();

        throw new Exception($"Несоответствие типов (тип {Type} и {other.Type}).");
    }
}
