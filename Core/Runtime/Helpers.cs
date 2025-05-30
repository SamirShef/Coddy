using Core.Values;

namespace Core.Runtime;

public static class Helpers
{
    public static IValue GetDefaultValue(TypeValue type) => type switch
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
