using Core.Runtime;

namespace Core.Values;

public class ObjectValue(ClassInfo classInfo) : IValue
{
    public ClassInfo ClassInfo { get; } = classInfo;
    public Dictionary<string, IValue> InstanceFields { get; } = [];
    public object Value { get => ClassInfo; set => throw new Exception("Нельзя модифицировать экземпляр класса."); }
    public TypeValue Type => TypeValue.Class;

    public void InitializeFields()
    {
        foreach (var field in ClassInfo.Fields)
        {
            InstanceFields[field.Key] = field.Value.Value ?? GetDefaultValue(field.Value.Type);
        }
    }

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
