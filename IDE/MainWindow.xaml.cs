using Microsoft.Win32;
using System.IO;
using System.Xml;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Coddy.IDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private string? currentFilePath;
        private bool isModified = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadSyntaxHighlighting();
            editor.Encoding = Encoding.UTF8;
            editor.TextChanged += Editor_TextChanged;

            // Добавляем обработчики горячих клавиш
            var newCommand = new RoutedCommand();
            newCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(newCommand, (s, e) => btnNew_Click(s, e)));

            var openCommand = new RoutedCommand();
            openCommand.InputGestures.Add(new KeyGesture(Key.O, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(openCommand, (s, e) => btnOpen_Click(s, e)));

            var saveCommand = new RoutedCommand();
            saveCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(saveCommand, (s, e) => btnSave_Click(s, e)));

            var runCommand = new RoutedCommand();
            runCommand.InputGestures.Add(new KeyGesture(Key.F5));
            CommandBindings.Add(new CommandBinding(runCommand, (s, e) => btnRun_Click(s, e)));
        }

        private static string GetSyntaxFilePath()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string syntaxDir = Path.Combine(appData, "Quantum Games Studio", "Coddy IDE", "Syntax");
            Directory.CreateDirectory(syntaxDir);
            string syntaxFile = Path.Combine(syntaxDir, "CoddySyntax.xshd");
            string installSyntaxFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Syntax", "CoddySyntax.xshd");
            if (File.Exists(installSyntaxFile)) File.Copy(installSyntaxFile, syntaxFile, true);
            
            return syntaxFile;
        }

        private void LoadSyntaxHighlighting()
        {
            try
            {
                using (Stream s = new FileStream(GetSyntaxFilePath(), FileMode.Open))
                {
                    using (XmlTextReader reader = new(s))
                    {
                        editor.SyntaxHighlighting = 
                            HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки подсветки синтаксиса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            isModified = true;
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            string fileName = string.IsNullOrEmpty(currentFilePath) ? "Новый файл" : Path.GetFileName(currentFilePath);
            Title = $"Coddy IDE - {fileName}{(isModified ? " *" : "")}";
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show(
                    "Сохранить изменения перед созданием нового файла?",
                    "Несохраненные изменения",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Cancel)
                    return;
                if (result == MessageBoxResult.Yes)
                    btnSave_Click(sender, e);
            }

            editor.Text = string.Empty;
            currentFilePath = null;
            isModified = false;
            UpdateWindowTitle();
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (isModified)
            {
                var result = MessageBox.Show(
                    "Сохранить изменения перед открытием нового файла?",
                    "Несохраненные изменения",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Cancel)
                    return;
                if (result == MessageBoxResult.Yes)
                    btnSave_Click(sender, e);
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Coddy Files (*.cd)|*.cd|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                currentFilePath = openFileDialog.FileName;
                editor.Text = File.ReadAllText(currentFilePath, Encoding.UTF8);
                isModified = false;
                UpdateWindowTitle();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Coddy Files (*.cd)|*.cd|All Files (*.*)|*.*",
                    DefaultExt = "cd"
                };

                if (saveFileDialog.ShowDialog() == true) currentFilePath = saveFileDialog.FileName;
                else return;
            }

            File.WriteAllText(currentFilePath, editor.Text, Encoding.UTF8);
            isModified = false;
            UpdateWindowTitle();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                MessageBox.Show("Пожалуйста, сначала сохраните файл перед выполнением кода.", "Файл не сохранен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                btnSave_Click(sender, e);

                System.Diagnostics.ProcessStartInfo startInfo = new()
                {
                    FileName = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName,
                    Arguments = $"--run \"{currentFilePath}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false
                };

                System.Diagnostics.Process.Start(startInfo);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка при выполнении кода:\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}
