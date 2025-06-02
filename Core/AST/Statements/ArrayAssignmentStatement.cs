using Core.Expressions;
using Core.Runtime;
using Core.Values;

namespace Core.AST.Statements;

public class ArrayAssignmentStatement(VariableStorage variableStorage, string name, IExpression index, IExpression expression) : IStatement
{
    public string Name { get; } = name;
    public IExpression Index { get; } = index;
    public IExpression Expression { get; } = expression;

    public void Execute()
    {
        if (!variableStorage.Exist(Name)) throw new Exception($"Невозможно получить значение массива: массив с именем {Name} не объявлен.");

        VariableInfo arrayInfo = variableStorage.Get(Name);
        if (arrayInfo.Value is not ArrayValue av) throw new Exception($"Невозможно получить значение массива: переменная с именем {Name} не является массивом.");

        IValue indexValue = Index.Evaluate();
        if (indexValue is not IntValue iv) throw new Exception($"Невозможно получить значение массива: индекс элемента не является целым числом. Сейчас индекс элемента имеет тип {indexValue.Type}.");
        if (iv.AsInt() < 0) throw new Exception($"Невозможно получить значение массива: индекс элемента не должен быть отрицательным. Сейчас индекс элемент имеет значение {iv.AsInt()}.");
        if (iv.AsInt() >= av.AsArray().Length) throw new Exception($"Невозможно получить значение массива: индекс элемента вышел за пределы массива {Name}.");

        IValue value = Expression.Evaluate();
        if (value.Type != av.ElementsType) throw new Exception($"Присвоение невозможно: тип массива {av.ElementsType} не соответствует типу переданного выражения {value.Type}.");

        av.AsArray()[iv.AsInt()] = value;

        variableStorage.Set(Name, arrayInfo.Value.Type, av);
    }
}
