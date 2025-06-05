using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace IDE
{
    public static class CodeRunner
    {
        public static void RunGeneratedCode(string sourceCode)
        {
            try
            {
                Console.WriteLine("Подготовка перед выполнением...");
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

                var references = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)).Select(a => MetadataReference.CreateFromFile(a.Location)).ToList();

                var compilation = CSharpCompilation.Create("GeneratedAssembly", [syntaxTree], references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.GetMessage());
                    throw new Exception($"Ошибки компиляции:\n{string.Join("\n", errors)}");
                }

                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());

                var programType = assembly.GetType("Program") ?? throw new Exception("Не удалось найти класс Program");
                var mainMethod = programType.GetMethod("__Main__", BindingFlags.Public | BindingFlags.Static) ?? throw new Exception("Не удалось найти метод __Main__()");

                Console.WriteLine("Выполнение");
                mainMethod.Invoke(null, null);
            }
            catch (Exception ex) { throw new Exception($"Ошибка при выполнении кода: {ex.Message}", ex); }
        }
    }
} 