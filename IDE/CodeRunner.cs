using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace IDE
{
    public static class CodeRunner
    {
        public static void RunGeneratedCode(string sourceCode)
        {
            try
            {
                Console.WriteLine("Preparation for execution...");
                // Создаем синтаксическое дерево из исходного кода
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

                // Получаем все сборки из текущего домена
                var references = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .Select(a => MetadataReference.CreateFromFile(a.Location))
                    .ToList();

                // Создаем компиляцию
                var compilation = CSharpCompilation.Create(
                    "GeneratedAssembly",
                    [syntaxTree],
                    references,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                );

                // Компилируем код в память
                using var ms = new MemoryStream();
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var errors = result.Diagnostics
                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                        .Select(d => d.GetMessage());
                    throw new Exception($"Ошибки компиляции:\n{string.Join("\n", errors)}");
                }

                // Загружаем скомпилированную сборку
                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());

                // Находим и вызываем метод __Main__
                var programType = assembly.GetType("Program");
                if (programType == null)
                {
                    throw new Exception("Не удалось найти класс Program");
                }

                var mainMethod = programType.GetMethod("__Main__", BindingFlags.Public | BindingFlags.Static);
                if (mainMethod == null)
                {
                    throw new Exception("Не удалось найти метод __Main__");
                }

                Console.WriteLine("Executing");
                mainMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при выполнении кода: {ex.Message}", ex);
            }
        }
    }
} 