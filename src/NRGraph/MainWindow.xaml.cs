using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

using GraphLib;
using GraphLib.UIClasses;

namespace NRGraph
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // MEMBERS

        private string fileToLoad;

        private GraphHistory graphHistory;

        // METHODS

        public MainWindow(string FileToLoad = "")
        {
            InitializeComponent();

            fileToLoad = FileToLoad;

            graphHistory = new GraphHistory();

            HistorySnapshop();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (fileToLoad != "")
                LoadGraph(fileToLoad);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            switch (MessageBox.Show("Сохранить?", "Выход", MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
            {
                case MessageBoxResult.Yes:
                    ApplicationCommands.Save.Execute(null, this);
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
            }
        }

        private void graphCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIGraphNode underMouseUIGraphNode = graphCanvas.GetUnderMouseUIGraphNode();
            UIGraphWay underMouseUIGraphWay = graphCanvas.GetUnderMouseUIGraphWay();
            
            if (underMouseUIGraphWay == null)
            {
                if (underMouseUIGraphNode == null)
                {
                    graphCanvas.AddUIGraphNode(new UIGraphNode(e.GetPosition(graphCanvas)));

                    HistorySnapshop();
                }
                else
                {
                    graphCanvas.Draging = underMouseUIGraphNode;
                    graphCanvas.DragingOffset = new Point(e.GetPosition(graphCanvas).X - underMouseUIGraphNode.GetCenter().X, e.GetPosition(graphCanvas).Y - underMouseUIGraphNode.GetCenter().Y);
                    graphCanvas.Draging.CaptureMouse();
                }
            }
            else
            {
                var wayEditor = new WayEditorWindow(underMouseUIGraphWay);

                wayEditor.Owner = this;
                wayEditor.ShowDialog();

                if (wayEditor.Modified)
                {
                    underMouseUIGraphWay.Update();

                    HistorySnapshop();
                }
            }
        }

        private void graphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (graphCanvas.Draging != null)
            {
                graphCanvas.Draging.Position = new Point( e.GetPosition(graphCanvas).X - graphCanvas.DragingOffset.X, e.GetPosition(graphCanvas).Y - graphCanvas.DragingOffset.Y);
                graphCanvas.UpdateGraph();
            }
        }

        private void graphCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (graphCanvas.Draging != null)
            {
                graphCanvas.Draging = null;                
            }
        }

        private void graphCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            UIGraphNode underMouseUIGraphNode = graphCanvas.GetUnderMouseUIGraphNode();
            UIGraphWay underMouseUIGraphWay = graphCanvas.GetUnderMouseUIGraphWay();

            if (underMouseUIGraphNode != null)
            {
                if (underMouseUIGraphNode.ToggleMark()) // если вернется true значит вершина помечается, иначе пометка снимается
                {
                    if (AutomatAddWayMode.IsChecked && Commands.AppCommands.AddWay.CanExecute(null,this))
                    {
                        Commands.AppCommands.AddWay.Execute(null, this);
                    }
                }

                HistorySnapshop();
            }
            else if (underMouseUIGraphWay != null)
            {
                graphCanvas.RemoveUIGraphWay(underMouseUIGraphWay);

                HistorySnapshop();
            }
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (graphCanvas.Graph.MarkedNodes.Count != 0);
        }

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (UIGraphNode item in graphCanvas.Graph.MarkedNodes.ToList())
            {
                graphCanvas.RemoveUIGraphNode(item);
            }

            HistorySnapshop();
        }

        private void RunAlgorithm_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (algorithmList.SelectedItem != null);
        }

        private void RunAlgorithm_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RunAlgorithm(algorithmList.SelectedItem as Algorithm);
        }

        private void AddWay_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (graphCanvas.Graph.MarkedNodes.Count == 2);
        }

        private void AddWay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var marked = graphCanvas.Graph.MarkedNodes;

            var first = marked[0] as UIGraphNode;
            var second = marked[1] as UIGraphNode;

            graphCanvas.AddUIGraphWayWithUnmark(new UIGraphWay(first, second));
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // сериализуем граф

            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "Файлы графов (*.nrgraph)|*.nrgraph";

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = sfd.FileName;
                SaveGraph(fileName);
            }
        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // десериализуем граф

            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "Файлы графов (*.nrgraph)|*.nrgraph";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string fileName = ofd.FileName;
                LoadGraph(fileName);
            }
        }

        private void Exit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void New_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show("Создать пустой граф?", "Новый граф", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                graphCanvas.Graph = new UIGraph();

                graphCanvas.DrawGraph(true);

                graphHistory.Clear();
                HistorySnapshop();
            }
        }

        private void Undo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = graphHistory.CanUndo();
        }

        private void Redo_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = graphHistory.CanRedo();
        }

        private void Undo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            graphCanvas.Graph.LoadFromGraph(graphHistory.HistoryUndo());

            graphCanvas.DrawGraph(true);
        }

        private void Redo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            graphCanvas.Graph.LoadFromGraph(graphHistory.HistoryRedo());

            graphCanvas.DrawGraph(true);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new About();

            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        ///////////////////////////////////////////////////////////////////////////////

        private void LoadGraph(string fileName)
        {
            byte[] data = File.ReadAllBytes(fileName);

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(data);

            try
            {
                Graph graph = (Graph)formatter.Deserialize(ms);
                
                graphCanvas.Graph.LoadFromGraph(graph);
                
                graphCanvas.DrawGraph(true);

                graphHistory.Clear();
                HistorySnapshop();
            }
            catch (Exception)
            {
                MessageBox.Show("Ошибка десериализации", "Файл повреждён", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveGraph(string fileName)
        {
            #region Сериализуем граф
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, graphCanvas.Graph.Clone());
            #endregion

            File.WriteAllBytes(fileName, stream.ToArray());
        }

        private void RunAlgorithm(Algorithm algorithm)
        {
            object result;
            #region Пытаемся выполнить алгоритм, результат пишем в result
            try
            {
                var copyOfGraph = graphCanvas.Graph.Clone();

                result = algorithm.Execute(copyOfGraph);
            }
            catch (AlgorithmException ex)
            {
                MessageBox.Show(ex.Message == "" ? "Граф или параметры не является допустимыми для данного алгоритма" : ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion

            #region Расшифровываем результат, и делаем в соответствии с типом необходимые действия
            if (algorithm is IReturnGraphWithText)
            {
                var graphWithText = (algorithm as IReturnGraphWithText).Decrypt(result);

                graphCanvas.Graph.LoadFromGraph(graphWithText.Graph);
                graphCanvas.DrawGraph(true);

                HistorySnapshop(graphWithText.Graph);

                MessageBox.Show(graphWithText.Text, "Текстовый результат выполнения", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (algorithm is IReturnGraph)
            {
                var graph = (algorithm as IReturnGraph).Decrypt(result);

                graphCanvas.Graph.LoadFromGraph(graph);
                graphCanvas.DrawGraph(true);

                HistorySnapshop(graph);
            }
            #endregion
        }

        private void HistorySnapshop()
        {
            HistorySnapshop(graphCanvas.Graph);
        }

        private void HistorySnapshop(Graph graph)
        {
            graphHistory.HistoryShot(graph.Clone());
        }
    }

    public class AlgorithmInfoTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            if (str == "")
                return "Пусто";
            else
                return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
