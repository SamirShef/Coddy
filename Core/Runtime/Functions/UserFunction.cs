using Core.AST.Statements;
using Core.Values;

namespace Core.Runtime.Functions;

public class UserFunction(string name, TypeValue returnType, List<(string, TypeValue)> parameters, IStatement body, VariableStorage variableStorage) : IFunction
{
    public TypeValue ReturnType { get; } = returnType;
    private readonly List<(string, TypeValue)> parameters = parameters;
    private readonly IStatement body = body;
    private readonly VariableStorage variableStorage = variableStorage;

    public IValue Execute(params IValue[] args)
    {
        if (args.Length != parameters.Count) throw new Exception($"Функция/метод '{name}()' ожидала {parameters.Count} аргумент, а получила {args.Length}.");

        variableStorage.EnterScope();
        try
        {
            for (int i = 0; i < parameters.Count; i++)
            {
                var (name, type) = parameters[i];
                var argValue = args[i];

                if (!Parser.Parser.IsTypeCompatible(type, argValue.Type)) throw new Exception($"Несовместимый тип аргумента '{name}'.");

                variableStorage.Declare(name, type, argValue);
            }

            body.Execute();

            if (ReturnType != TypeValue.Void) throw new Exception("Не все пути возвращают значения.");
        }
        catch (ReturnException ret)
        {
            if (ReturnType == TypeValue.Void && ret.Value != null) throw new Exception("Функция/метод не должен возвращать значение.");

            if (ReturnType != TypeValue.Void && ret.Value == null) throw new Exception("Не все пути возвращают значения.");

            if (ReturnType != TypeValue.Void && !Parser.Parser.IsTypeCompatible(ReturnType, ret.Value!.Type)) throw new Exception($"Несовместимый тип возвращаемого значения: ожидается {ReturnType}, возвращено {ret.Value!.Type}.");

            return ret.Value ?? new VoidValue();
        }
        finally { variableStorage.ExitScope(); }

        return ReturnType == TypeValue.Void ? new VoidValue() : throw new Exception("Не все пути возвращают значения.");
    }
}
