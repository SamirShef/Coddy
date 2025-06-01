using Core.AST.Statements;
using Core.Runtime.OOP;
using Core.Values;

namespace Core.Runtime.Functions;

public class UserFunction(string name, string returnTypeValue, TypeValue returnType, List<(string, string, TypeValue)> parameters, IStatement body, VariableStorage variableStorage, ClassInfo? classInfo = null) : IFunction
{
    private readonly List<(string, string, TypeValue)> parameters = parameters;
    private readonly IStatement body = body;
    private readonly VariableStorage variableStorage = variableStorage;
    private readonly ClassInfo? classInfo = classInfo;

    public string ReturnTypeValue { get; } = returnTypeValue;
    public TypeValue ReturnType { get; } = returnType;
    public List<(string, string, TypeValue)> Parameters { get; } = parameters;
    public IStatement Body { get; } = body;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != parameters.Count) throw new Exception($"Функция '{name}()' ожидала {parameters.Count} аргументов, а получила {args.Length}.");

        variableStorage.EnterScope();
        try
        {
            if (classInfo is not null) variableStorage.Declare("this", TypeValue.Class, new ClassValue(new ClassInstance(classInfo)));

            for (int i = 0; i < parameters.Count; i++)
            {
                var (name, typeValue, type) = parameters[i];
                var argValue = args[i];

                if (!Parser.Parser.IsTypeCompatible(type, argValue.Type)) throw new Exception($"Несовместимый тип аргумента '{name}'.");

                variableStorage.Declare(name, type, Parser.Parser.ConvertValue(argValue, type));
            }

            body.Execute();

            if (ReturnType != TypeValue.Void) throw new Exception("Не все пути возвращают значения.");
        }
        catch (ReturnException ret)
        {
            if (ReturnType == TypeValue.Void && ret.Value != null) throw new Exception("Функция не должен возвращать значение.");

            if (ReturnType != TypeValue.Void && ret.Value == null) throw new Exception("Не все пути возвращают значения.");

            if (ReturnType != TypeValue.Void && !Parser.Parser.IsTypeCompatible(ReturnType, ret.Value!.Type)) throw new Exception($"Несовместимый тип возвращаемого значения: ожидается {ReturnType}, возвращено {ret.Value!.Type}.");

            return ret.Value ?? new VoidValue();
        }
        finally { variableStorage.ExitScope(); }

        return ReturnType == TypeValue.Void ? new VoidValue() : throw new Exception("Не все пути возвращают значения.");
    }
}
