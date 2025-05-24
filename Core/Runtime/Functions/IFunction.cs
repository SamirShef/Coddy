using Core.Values;

namespace Core.Runtime.Functions;

public interface IFunction
{
    TypeValue ReturnType { get; }
    IValue Execute(params IValue[] args);
}
