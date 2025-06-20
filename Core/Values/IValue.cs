namespace Core.Values;

public interface IValue
{
    object Value { get; set; }
    TypeValue Type { get; }
    IValue Add(IValue other);
    IValue Subtract(IValue other);
    IValue Multiply(IValue other);
    IValue Divide(IValue other);
    IValue Modulo(IValue other);
    IValue Greater(IValue other);
    IValue GreaterEqual(IValue other);
    IValue Less(IValue other);
    IValue LessEqual(IValue other);

    IValue And(IValue other);
    IValue Or(IValue other);
    IValue Equals(IValue other);
    IValue NotEquals(IValue other);
    IValue LeftShift(IValue other);
    IValue RightShift(IValue other);
    IValue LogicalRightShift(IValue other);
    string AsString();
}

public enum TypeValue
{
    Int, Float, Double, Decimal, String, Bool, Char, Array, Void, Class, Enum,
}