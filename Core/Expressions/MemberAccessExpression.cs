using Core.Values;

namespace Core.Expressions;

public class MemberAccessExpression(IExpression objectExpr, string name) : IExpression
{
    private readonly IExpression objectExpr = objectExpr;
    private readonly string name = name;

    public IValue Evaluate()
    {
        var obj = objectExpr.Evaluate() as ObjectValue ?? throw new Exception("Ожидался объект.");

        if (obj.InstanceFields.TryGetValue(name, out var value)) return value;

        if (obj.ClassInfo.Fields.TryGetValue(name, out var field)) return field.Value;

        if (obj.ClassInfo.Methods.TryGetValue(name, out var method)) return new MethodReferenceValue(method);

        throw new Exception($"Член '{name}' не был найдет в классе.");
    }
}
