using Core.Values;

namespace Core.Runtime.Functions;

public interface IFunction
{
    string ReturnType { get; }
}
