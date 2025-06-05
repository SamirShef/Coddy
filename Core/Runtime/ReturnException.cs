using Core.Values;

namespace Core.Runtime;

public class ReturnException(IValue? value) : Exception
{
    public IValue? Value { get; } = value;
}
