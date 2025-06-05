using Core.Runtime;
using Core.Values;

namespace Core.Expressions;

public class ArrayExpression(VariableStorage variableStorage, string name, IExpression index) : IExpression
{
    public string Name { get; } = name;
    public IExpression Index { get; } = index;

    public IValue Evaluate()
    {
        if (!variableStorage.Exist(Name)) throw new Exception($"Невозможно получить значение массива: массив с именем {Name} не объявлен.");

        VariableInfo arrayInfo = variableStorage.Get(Name);
        if (arrayInfo.Value is not ArrayValue av) throw new Exception($"Невозможно получить значение массива: переменная с именем {Name} не является массивом.");

        IValue indexValue = Index.Evaluate();
        if (indexValue is not IntValue iv) throw new Exception($"Невозможно получить значение массива: индекс элемента не является целым числом. Сейчас индекс элемента имеет тип {indexValue.Type}.");
        if (iv.AsInt() < 0) throw new Exception($"Невозможно получить значение массива: индекс элемента не должен быть отрицательным. Сейчас индекс элемент имеет значение {iv.AsInt()}.");
        if (iv.AsInt() >= av.AsArray().Length) throw new Exception($"Невозможно получить значение массива: индекс элемента вышел за пределы массива {Name}.");

        return av.AsArray()[iv.AsInt()];
    }
}
