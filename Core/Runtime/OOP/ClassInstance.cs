using Core.Values;

namespace Core.Runtime.OOP;

public class ClassInstance
{
    private readonly ClassInfo classInfo;
    private readonly Dictionary<string, IValue> fields = [];

    public ClassInstance(ClassInfo classInfo)
    {
        this.classInfo = classInfo;

        foreach (var field in classInfo.Fields) fields.Add(field.Key, field.Value.Value);
    }

    public IValue GetFieldValue(string name)
    {
        if (!classInfo.Fields.ContainsKey(name)) throw new Exception($"Невозможно получить значение поля: поле с именем '{name}' в классе '{classInfo.Name}' не объявлено.");
        if (classInfo.Fields[name].Access == AccessModifier.Private) throw new Exception($"Невозможно получить значение поля: поле с именем '{name}' в классе '{classInfo.Name}' помечено как защищенное.");

        return fields[name];
    }

    public void SetFieldValue(string name, IValue value)
    {
        if (!classInfo.Fields.ContainsKey(name)) throw new Exception($"Невозможно присвоить новое значение полю: поле с именем '{name}' в классе '{classInfo.Name}' не объявлено.");
        if (classInfo.Fields[name].Access == AccessModifier.Private) throw new Exception($"Невозможно присвоить новое значение полю: поле с именем '{name}' в классе '{classInfo.Name}' помечено как защищенное.");
        if (classInfo.Fields[name].Type != value.Type) throw new Exception($"Невозможно присвоить новое значение полю: несоответствие типов ('{classInfo.Fields[name].Type}' и '{value.Type}').");

        fields[name] = value;
    }
}
