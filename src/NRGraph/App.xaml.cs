using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace NRGraph
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_Startup(object sender, StartupEventArgs e)
        {
            if (File.Exists("GraphLib.dll"))
            {
                MainWindow main = new MainWindow(e.Args.Length == 1 ? e.Args[0] : "");
                main.Show();
            }
            else
            {
                MessageBox.Show("Отсутствует файл сборки GraphLib.dll", "Ошибка инициализации приложения", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }
    }
}
