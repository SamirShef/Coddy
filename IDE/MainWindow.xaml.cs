﻿using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32")]
        public static extern void AllocConsole();

        public MainWindow()
        {
            InitializeComponent();
            AllocConsole();

            Compiler.Execute(File.ReadAllText("H:\\MyProjects\\Coddy\\Core\\Program.cd"));
        }
    }
}