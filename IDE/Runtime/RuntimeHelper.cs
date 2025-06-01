namespace IDE.Runtime;

public static class RuntimeHelper
{
    public static void Println(object message) => Console.WriteLine(message);

    public static string Input()
    {
        string? message = Console.ReadLine();
        return message != null ? message : "";
    }

    public static int ToInt(object arg)
    {
        if (arg is not string sv) throw new Exception($"Функция 'to_int()' ожидала тип аргумента {arg.GetType()}, а получила String.");
        return int.Parse(sv);
    }

    public static float ToFloat(object arg)
    {
        if (arg is not string sv) throw new Exception($"Функция 'to_float()' ожидала тип аргумента {arg.GetType()}, а получила String.");
        return float.Parse(sv.Replace(".", ","));
    }

    public static double ToDouble(object arg)
    {
        if (arg is not string sv) throw new Exception($"Функция 'to_double()' ожидала тип аргумента {arg.GetType()}, а получила String.");
        return double.Parse(sv.Replace(".", ","));
    }

    public static decimal ToDecimal(object arg)
    {
        if (arg is not string sv) throw new Exception($"Функция 'to_decimal()' ожидала тип аргумента {arg.GetType()}, а получила String.");
        return decimal.Parse(sv.Replace(".", ","));
    }

    public static string ToString(object arg) => arg.ToString() ?? "";

    public static bool ToBoolean(object arg)
    {
        if (arg is not string sv) throw new Exception($"Функция 'to_boolean()' ожидала тип аргумента {arg.GetType()}, а получила String.");
        if (sv != "false" && sv != "true") throw new Exception($"Строковый литерал '{sv}' невозможно преобразовать в тип boolean");
        return bool.Parse(sv);
    }
}
