using System.Reflection;
using Core.Runtime.OOP;
using Core.Values;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Core.Expressions;
using Core.Runtime;

namespace Core.AST.Statements;

public class IncludeStatement(ClassStorage classStorage, string libraryPath) : IStatement
{
    public ClassStorage ClassStorage { get; } = classStorage;
    public string LibraryPath { get; } = libraryPath;

    private static string GetProjectRootPath()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        // Поднимаемся на 4 уровня вверх от IDE/bin/Debug/net8.0-windows до корня проекта
        return Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", ".."));
    }

    public void Execute()
    {
        string fullPath = Path.Combine(GetProjectRootPath(), "Core", "Libraries", LibraryPath);
        if (!File.Exists(fullPath)) throw new Exception($"Библиотека не найдена: {fullPath}");

        string source = File.ReadAllText(fullPath);

        string extension = Path.GetExtension(fullPath).ToLower();
        switch (extension)
        {
            case ".cd": ExecuteCoddyLibrary(source); break;
            case ".dll": ExecuteDLLLibrary(fullPath); break;
            case ".cs": ExecuteCSharpLibrary(fullPath); break;
            default: throw new Exception($"Файл с расширением '{extension}' не является поддерживаемым библиотечным файлом. Поддерживаемые расширения: .cd, .dll, .cs");
        }
    }

    private void ExecuteCoddyLibrary(string source)
    {
        try
        {
            Lexer.Lexer lexer = new(source);
            Parser.Parser parser = new([.. lexer.Tokenize()]);
            List<IStatement> statements = parser.Parse();

            foreach (IStatement statement in statements)
            {
                if (statement is IncludeStatement includeStatement) includeStatement.Execute();
                if (statement is ClassDeclarationStatement classDeclaration) classDeclaration.Execute();
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при выполнении Coddy библиотеки: {ex.Message}");
        }
    }

    private void ExecuteDLLLibrary(string dllPath)
    {
        try
        {
            Assembly assembly = Assembly.LoadFrom(dllPath);
            LoadAssemblyTypes(assembly);
        }
        catch (ReflectionTypeLoadException ex)
        {
            string loaderExceptions = string.Join("\n", ex.LoaderExceptions?.Select(e => e?.Message) ?? Array.Empty<string>());
            throw new Exception($"Ошибка при загрузке типов из DLL: {loaderExceptions}");
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при выполнении DLL библиотеки: {ex.Message}");
        }
    }

    private void ExecuteCSharpLibrary(string csPath)
    {
        try
        {
            string tempDllPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".dll");
            
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(csPath));
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
            };

            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(csPath),
                [syntaxTree],
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var result = compilation.Emit(tempDllPath);
            if (!result.Success)
            {
                var errors = string.Join("\n", result.Diagnostics.Select(d => d.GetMessage()));
                throw new Exception($"Ошибка компиляции C# библиотеки:\n{errors}");
            }

            Assembly assembly = Assembly.LoadFrom(tempDllPath);
            LoadAssemblyTypes(assembly);

            try { File.Delete(tempDllPath); } catch { }
        }
        catch (Exception ex) { throw new Exception($"Ошибка при выполнении C# библиотеки: {ex.Message}"); }
    }

    private void LoadAssemblyTypes(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsPublic || type.IsNestedPublic)
            {
                ClassInfo classInfo = new(type.Name)
                {
                    IsStatic = type.IsAbstract && type.IsSealed && !type.IsInterface
                };

                foreach (System.Reflection.MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (!method.IsSpecialName)
                    {
                        var parameters = method.GetParameters().Select(p => (p.Name ?? "param", p.ParameterType.Name, ConvertPrimaryToCoddyTypeValue(p.ParameterType))).ToList();
                        var returnType = ConvertPrimaryToCoddyTypeValue(method.ReturnType);
                        
                        var methodBody = CreateMethodBody(method, type, parameters);
                        
                        var userFunction = new Runtime.Functions.UserFunction(method.Name, type.Name, returnType, parameters, methodBody, new VariableStorage(), classInfo, method.IsStatic);
                        var methodInfo = new Runtime.OOP.MethodInfo(AccessModifier.Public, userFunction, method.IsStatic);
                        classInfo.AddMethod(method.Name, methodInfo);
                    }
                }

                foreach (System.Reflection.FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    var fieldType = ConvertPrimaryToCoddyTypeValue(field.FieldType);
                    var fieldValue = field.GetValue(null);
                    var fieldInfo = new Runtime.OOP.FieldInfo(fieldType, AccessModifier.Public, fieldValue != null ? ConvertPrimaryToCoddyValue(fieldValue) : null, field.IsStatic);
                    classInfo.AddField(field.Name, fieldInfo);
                }

                ClassStorage.Declare(type.Name, classInfo);
            }
        }
    }

    private IStatement CreateMethodBody(System.Reflection.MethodInfo method, Type type, List<(string name, string typeValue, TypeValue type)> parameters)
    {
        var statements = new List<IStatement>();
        var variableStorage = new VariableStorage();

        foreach (var param in parameters) statements.Add(new VariableDeclarationStatement(variableStorage, classStorage, param.name, param.typeValue, param.type, null));

        var methodCall = new MethodCallExpression(method.IsStatic ? null : new VariableExpression(variableStorage, "this"), method.Name, [.. parameters.Select(p => new VariableExpression(variableStorage, p.name))]);

        if (method.ReturnType != typeof(void)) statements.Add(new ReturnStatement(methodCall));
        else statements.Add(new MethodCallStatement(methodCall));

        return new BlockStatement(statements);
    }

    private TypeValue ConvertPrimaryToCoddyTypeValue(object value) => value switch
    {
        int => TypeValue.Int,
        float => TypeValue.Float,
        double => TypeValue.Double,
        decimal => TypeValue.Decimal,
        string => TypeValue.String,
        bool => TypeValue.Bool,
        _ => TypeValue.Class,
    };

    private IValue ConvertPrimaryToCoddyValue(object value) => value switch
    {
        int i => new IntValue(i),
        float f => new FloatValue(f),
        double d => new DoubleValue(d),
        decimal m => new DecimalValue(m),
        string s => new StringValue(s),
        bool b => new BoolValue(b),
        _ => throw new NotSupportedException($"Тип {value?.GetType().Name} не поддерживается для конвертации в Coddy значение")
    };
}