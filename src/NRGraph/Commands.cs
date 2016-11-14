using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace NRGraph
{
    namespace Commands
    {
        public class AppCommands
        {
            public static RoutedUICommand RunAlgorithm;
            public static RoutedUICommand DeleteNode;
            public static RoutedUICommand AddWay;
            public static RoutedUICommand Exit;

            static AppCommands()
            {
                RunAlgorithm = new RoutedUICommand( "Запустить алгоритм", "Запустить алгоритм", typeof(AppCommands) );
                DeleteNode = new RoutedUICommand("Удалить вершины", "Удалить вершины", typeof(AppCommands));
                AddWay = new RoutedUICommand("Добавить путь", "Добавить путь", typeof(AppCommands));
                Exit = new RoutedUICommand("Выход", "Выход", typeof(AppCommands));
            }
        }
    }
}
