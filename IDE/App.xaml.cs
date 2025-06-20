using System.Windows;
using System.Text;
using System.Runtime.InteropServices;

namespace Coddy.IDE
{
    public partial class App : Application
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length > 0 && e.Args[0] == "--run" && e.Args.Length > 1)
            {
                string filePath = e.Args[1];
                if (System.IO.File.Exists(filePath))
                {
                    try
                    {
                        AllocConsole();
                        var consoleWindow = GetConsoleWindow();
                        ShowWindow(consoleWindow, SW_SHOW);

                        Console.OutputEncoding = Encoding.UTF8;
                        Console.SetOut(new System.IO.StreamWriter(Console.OpenStandardOutput(), Encoding.UTF8) { AutoFlush = true });
                        Console.SetError(new System.IO.StreamWriter(Console.OpenStandardError(), Encoding.UTF8) { AutoFlush = true });

                        string code = System.IO.File.ReadAllText(filePath);
                        Compiler.Execute(code, filePath);

                        Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                        Console.ReadKey();

                        ShowWindow(consoleWindow, SW_HIDE);
                        FreeConsole();
                    }
                    catch (Exception ex)
                    {
                        var consoleWindow = GetConsoleWindow();
                        ShowWindow(consoleWindow, SW_HIDE);
                        FreeConsole();

                        MessageBox.Show($"Ошибка при выполнении кода:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}", "Ошибка выполнения", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally { Shutdown(); }
                }
            }
        }
    }
}
