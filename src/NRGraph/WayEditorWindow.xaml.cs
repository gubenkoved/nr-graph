using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GraphLib;

namespace NRGraph
{
    /// <summary>
    /// Логика взаимодействия для WayEditorWindow.xaml
    /// </summary>
    public partial class WayEditorWindow : Window
    {
        public GraphWay Way { get; private set; }

        public bool Modified { get; private set; }

        public WayEditorWindow(GraphWay way)
        {
            InitializeComponent();

            Way = way;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WeightIsLen.IsChecked = Way.WeightIsLen;

            Weight.Text = Way.Weight.ToString("0.00");

            MarkWay.IsChecked = Way.Style == WayStyle.Marked;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            bool weightIsLen = WeightIsLen.IsChecked ?? false;
            bool markWay = MarkWay.IsChecked ?? false;

            Modified = weightIsLen != Way.WeightIsLen;

            Way.WeightIsLen = weightIsLen;

            if (!weightIsLen)
                Way.Weight = double.Parse(Weight.Text);

            Modified = Modified || markWay != (Way.Style == WayStyle.Marked);

            if (markWay)
                Way.Style = WayStyle.Marked;
            else
                Way.Style = WayStyle.Simple;

            Close();
        }

        private void Data_Changed(object sender, EventArgs e)
        {
            ValidateData();
        }

        private void ValidateData()
        {
            double d;

            SaveBtn.IsEnabled = (WeightIsLen.IsChecked ?? false) || double.TryParse(Weight.Text, out d) && d > 0.0;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
