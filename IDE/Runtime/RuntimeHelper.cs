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
        if (arg is string sv) return int.Parse(sv);
        if (arg is float fv) return (int)fv;
        if (arg is double dov) return (int)dov;
        if (arg is decimal dev) return (int)dev;

        throw new Exception($"Функция 'to_int()' ожидала тип аргумента string или float или double или decimal, а получила {arg.GetType().Name.ToLower()}.");
    }

    public static float ToFloat(object arg)
    {
        if (arg is string sv) return float.Parse(sv.Replace(".", ","));
        if (arg is int iv) return (float)iv;
        if (arg is double dov) return (float)dov;
        if (arg is decimal dev) return (float)dev;

        throw new Exception($"Функция 'to_float()' ожидала тип аргумента string или int или double или decimal, а получила {arg.GetType().Name.ToLower()}.");
    }

    public static double ToDouble(object arg)
    {
        if (arg is string sv) return double.Parse(sv.Replace(".", ","));
        if (arg is int iv) return (double)iv;
        if (arg is float fv) return (double)fv;
        if (arg is decimal dev) return (double)dev;

        throw new Exception($"Функция 'to_double()' ожидала тип аргумента string или int или float или decimal, а получила {arg.GetType().Name.ToLower()}.");
    }

    public static decimal ToDecimal(object arg)
    {
        if (arg is string sv) return decimal.Parse(sv.Replace(".", ","));
        if (arg is int iv) return (decimal)iv;
        if (arg is float fv) return (decimal)fv;
        if (arg is double dov) return (decimal)dov;

        throw new Exception($"Функция 'to_decimal()' ожидала тип аргумента string или int или float или double, а получила {arg.GetType().Name.ToLower()}.");
    }

    public static string ToString(object arg) => arg.ToString() ?? "";

    public static bool ToBoolean(object arg)
    {
        if (arg is not string sv) throw new Exception($"Функция 'to_boolean()' ожидала тип аргумента string, а получила {arg.GetType().Name.ToLower()}.");
        if (sv != "false" && sv != "true") throw new Exception($"Строковый литерал '{sv}' невозможно преобразовать в тип boolean");
        return bool.Parse(sv);
    }

    public static string GetType(object arg) => arg.GetType().Name;

    public static int GetLen(object arg)
    {
        if (arg is Array arr) return arr.Length;
        if (arg is string str) return str.Length;

        throw new Exception($"Функция 'len()' ожидала тип аргумента string или array, а получила {arg.GetType().Name.ToLower()}.");
    }
}
