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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphLib
{
    namespace UIClasses
    {
        /// <summary>
        /// Интерфейс поддерживающий рисование на Canvas
        /// </summary>
        public interface IDrawable
        {
            void Draw(Canvas canvas);
            void Update();
        }

        /// <summary>
        /// Класс представляющий вершину графа, которая может быть отрисована на Canvas
        /// </summary>
        public class UIGraphNode : GraphNode, IDrawable
        {
            // PRIVATE ACCESS

            private static double sideLen = 40;

            private UIElement uielement;

            private void makeVisual()
            {
                Grid grid = new Grid();
                grid.Height = grid.Width = sideLen;

                Ellipse ellipse = new Ellipse();
                ellipse.Stroke = new SolidColorBrush(Colors.White);
                ellipse.StrokeThickness = 3;      
                grid.Children.Add(ellipse);

                TextBlock text = new TextBlock();
                text.FontSize = 10;
                text.FontFamily = new FontFamily("Calibri");
                text.TextAlignment = TextAlignment.Center;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                text.Foreground = new SolidColorBrush(Colors.White);
                grid.Children.Add(text);

                grid.MouseEnter += new MouseEventHandler(grid_MouseEnter);
                grid.MouseLeave += new MouseEventHandler(grid_MouseLeave);
                grid.MouseUp += new MouseButtonEventHandler(grid_MouseUp);
                
                uielement = grid;

                updateVisual();
            }

            private void updateVisual()
            {
                Grid grid = uielement as Grid;
                Ellipse ellipse = grid.Children[0] as Ellipse;
                TextBlock textBlock = grid.Children[1] as TextBlock;

                string text = "";

                if (Name != "")
                    text += Name + (Info == "" ? "" : "\n");
                if (Info != "")
                    text += Info;


                textBlock.Text = text;

                Canvas.SetLeft(uielement, Position.X - sideLen / 2.0);
                Canvas.SetTop(uielement, Position.Y - sideLen / 2.0);

                switch (Style)
                {
                    case NodeStyle.Selected:
                        ellipse.Fill = new SolidColorBrush(Colors.Red);
                        break;
                    case NodeStyle.Simple:
                        ellipse.Fill = new SolidColorBrush(Colors.Black);
                        break;
                    case NodeStyle.Marked:
                        ellipse.Fill = new SolidColorBrush(Colors.Green);
                        break;
                }
            }

            private bool underMouse;

            // PUBLIC ACCESS

            public new Point Position
            {
                get { return base.Position; }
                
                set
                {
                    base.Position = new Point(value.X - sideLen / 2.0, value.Y - sideLen / 2.0);

                    updateVisual();
                }
            }

            public new NodeStyle Style
            {
                get { return base.Style; }
                
                set
                {
                    base.Style = value;

                    updateVisual();
                }
            }

            public UIGraphNode( Point NodePosition, string NodeName = "", string NodeInfo = "" )
                : base(new Point(NodePosition.X ,NodePosition.Y ), NodeName, NodeInfo)
            {
                makeVisual();
            }

            public void Draw(Canvas canvas)
            {
                if (!canvas.Children.Contains(uielement))
                    canvas.Children.Add(uielement);

                Update();
            }

            public void Update()
            {
                updateVisual();
            }

            public void CaptureMouse()
            {
                ((uielement as Grid).Children[0] as Ellipse).CaptureMouse();
            }

            public void ReleaseMouseCapture()
            {
                ((uielement as Grid).Children[0] as Ellipse).ReleaseMouseCapture();
            }

            public void Mark()
            {
                Style = NodeStyle.Marked;
            }

            public void Unmark()
            {
                Style = NodeStyle.Simple;
            }

            public bool ToggleMark()
            {
                if (Style == NodeStyle.Marked)
                {
                    Style = NodeStyle.Selected;
                    return false;
                }
                else
                {
                    Style = NodeStyle.Marked;
                    return true;
                }
            }

            public Point GetCenter()
            {
                return new Point(Position.X + sideLen / 2.0, Position.Y + sideLen / 2.0);
            }

            public bool UnderMouse
            {
                get
                {
                    return underMouse;
                }
            }

            // EVENTS

            void grid_MouseLeave(object sender, MouseEventArgs e)
            {
                underMouse = false;

                if (Style == NodeStyle.Selected)
                    Style = NodeStyle.Simple;
            }
            void grid_MouseEnter(object sender, MouseEventArgs e)
            {
                underMouse = true;

                if (Style == NodeStyle.Simple)
                    Style = NodeStyle.Selected;
            }
            void grid_MouseUp(object sender, MouseButtonEventArgs e)
            {
                ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// Класс представляющий путь в графе, который может быть отрисован на Canvas
        /// </summary>
        public class UIGraphWay : GraphWay, IDrawable
        {
            // PRIVATE ACCESS

            private static double indentionFromCenter = 24;

            private bool underMouse;

            private Dictionary<string, UIElement> visuals;

            private void makeVisual()
            {
                visuals = new Dictionary<string, UIElement>();

                #region Line
                var line = new Arrow();

                visuals.Add("line", line);
                line.StrokeThickness = 3;
                line.StrokeStartLineCap = PenLineCap.Round;
                line.StrokeEndLineCap = PenLineCap.Round;

                line.MouseEnter += new MouseEventHandler(line_MouseEnter);
                line.MouseLeave += new MouseEventHandler(line_MouseLeave);

                if (Oriented)
                {
                    line.HeadHeight = 7;
                    line.HeadWidth = 14;
                } 
                #endregion

                #region Text
                var text = new TextBlock();
                visuals.Add("textBlock", text);
                text.FontSize = 12;
                text.FontFamily = new FontFamily("Calibri");
                text.TextAlignment = TextAlignment.Center;
                text.HorizontalAlignment = HorizontalAlignment.Center;
                text.VerticalAlignment = VerticalAlignment.Center;
                text.Foreground = new SolidColorBrush(Colors.White);
                text.IsHitTestVisible = false;
                Panel.SetZIndex(text, 1);
                #endregion

                updateVisual();
            }

            private void updateVisual()
            {
                var line = visuals["line"] as Arrow;
                var textBlock = visuals["textBlock"] as TextBlock;

                #region Расчитываем положения элементов
                double angle = Math.Atan2(to.Position.Y - from.Position.Y, to.Position.X - from.Position.X);
                double xIndent = Math.Cos(angle) * indentionFromCenter;
                double yIndent = Math.Sin(angle) * indentionFromCenter;

                line.X1 = from.Position.X + xIndent;
                line.Y1 = from.Position.Y + yIndent;
                line.X2 = to.Position.X - xIndent;
                line.Y2 = to.Position.Y - yIndent;

                Canvas.SetLeft(textBlock, 0.5 * (line.X1 + line.X2));
                Canvas.SetTop(textBlock, 0.5 * (line.Y1 + line.Y2));
                #endregion

                if (WeightIsLen)
                    textBlock.Text = Weight.ToString("0") + "px";
                else
                    textBlock.Text = Weight.ToString("0.0");

                switch (style)
                {
                    case WayStyle.Simple:
                        textBlock.FontWeight = FontWeights.Bold;
                        textBlock.Foreground = new SolidColorBrush(Colors.Black);
                        line.Stroke = new SolidColorBrush(Colors.White);
                        break;
                    case WayStyle.Selected:
                        textBlock.FontWeight = FontWeights.Normal;
                        textBlock.Foreground = new SolidColorBrush(Colors.White);
                        line.Stroke = new SolidColorBrush(Colors.Black);
                        break;
                    case WayStyle.Marked:
                        line.Stroke = new SolidColorBrush(Colors.Green);
                        break;
                }
            }

            // PUBLIC ACCESS

            public new WayStyle Style
            {
                get
                {
                    return style;
                }
                set
                {
                    style = value;
                    updateVisual();
                }
            }

            public bool ToggleMark()
            {
                if (Style == WayStyle.Marked)
                {
                    Style = WayStyle.Selected;

                    return false;
                }
                else
                {
                    Style = WayStyle.Marked;

                    return true;
                }
            }

            public bool UnderMouse
            {
                get
                {
                    return underMouse;
                }
            }

            public UIGraphWay(UIGraphNode FromNode, UIGraphNode ToNode, bool oriented = true)
                : base(FromNode, ToNode, oriented)
            {
                makeVisual();
            }
            
            public void Draw(Canvas canvas)
            {
                foreach (var item in visuals)
                {
                    if (!canvas.Children.Contains(item.Value))
                        canvas.Children.Add(item.Value);
                }

                Update();
            }

            public void Update()
            {
                updateVisual();
            }

            public new UIGraphNode From
            {
                get
                {
                    return from as UIGraphNode;
                }
            }
            
            public new UIGraphNode To
            {
                get
                {
                    return to as UIGraphNode;
                }
            }

            // EVENTS
            void line_MouseEnter(object sender, MouseEventArgs e)
            {
                underMouse = true;

                if (Style != WayStyle.Marked)
                    Style = WayStyle.Selected;
            }
            void line_MouseLeave(object sender, MouseEventArgs e)
            {
                underMouse = false;

                if (Style != WayStyle.Marked)
                    Style = WayStyle.Simple;
            }
        }

        /// <summary>
        /// Класс представляющий граф, который может быть отрисован на Canvas
        /// </summary>
        public class UIGraph : Graph, IDrawable
        {
            public UIGraph()
                : base()
            {
            }

            public void Draw(Canvas canvas)
            {
                foreach (IDrawable item in Ways)
                {
                    item.Draw(canvas);
                }

                foreach (IDrawable item in Nodes)
                {
                    item.Draw(canvas);
                }

                Update();
            }

            public void Update()
            {
                foreach (IDrawable item in Ways)
                {
                    item.Update();
                }

                foreach (IDrawable item in Nodes)
                {
                    item.Update();
                }
            }

            public void LoadFromGraph(Graph graph)
            {
                // предотвращаем ошибки из за загрузки из себя же
                if (this == graph)
                    return;

                Clear();

                var nodeDictionary = new Dictionary<GraphNode, UIGraphNode>();

                foreach (GraphNode node in graph.Nodes)
                {
                    var newNode = new UIGraphNode(node.Position, node.Name, node.Info);
                    nodeDictionary[node] = newNode;

                    AddNode(newNode);
                }

                for (int i = 0; i < graph.MarkedNodes.Count; i++)
                {
                    nodeDictionary[graph.MarkedNodes[i]].Style = graph.MarkedNodes[i].Style;
                }

                foreach (GraphWay way in graph.Ways)
                {
                    var newWay = new UIGraphWay(nodeDictionary[way.From], nodeDictionary[way.To], way.Oriented);

                    if (!way.WeightIsLen)
                    {
                        newWay.WeightIsLen = false;
                        newWay.Weight = way.Weight;
                    }
                    newWay.Style = way.Style;

                    AddWay(newWay);
                }
            }
        }

        /// <summary>
        /// Canvas приспособленный под отображения UIGraph
        /// </summary>
        public class GraphCanvas : Canvas
        {
            // PUBLIC ACCESS

            public UIGraph Graph;

            public Point DragingOffset;

            public UIGraphNode Draging;

            public void AddUIGraphNode(UIGraphNode node)
            {
                Graph.AddNode(node);
                DrawGraph();
            }

            public void RemoveUIGraphNode(UIGraphNode node)
            {
                Graph.RemoveNode(node);
                
                DrawGraph(true);
            }

            public void AddUIGraphWayWithUnmark(UIGraphWay way)
            {
                AddUIGraphWay(way);

                way.To.Unmark();
                way.From.Unmark();
            }

            public void AddUIGraphWay(UIGraphWay way)
            {
                if (Graph.FindWay(way.From, way.To, true) == null)
                {
                    var reverseWay = Graph.FindWay(way.To, way.From, true);

                    if (reverseWay != null)
                    {
                        // make a non-oriented way, because exist way From->To and To->From
                        RemoveUIGraphWay(reverseWay as UIGraphWay);
                        AddUIGraphWay(new UIGraphWay(way.From, way.To, false));
                    }
                    else
                    {
                        Graph.AddWay(way);
                        DrawGraph();
                    }
                }
            }

            public void RemoveUIGraphWay(UIGraphWay way)
            {
                Graph.RemoveWay(way);

                DrawGraph(true);
            }

            public void DrawGraph(bool fullRedraw = false)
            {
                if (fullRedraw)
                    Children.Clear();

                Graph.Draw(this);
            }

            public void UpdateGraph()
            {
                Graph.Update();
            }

            public UIGraphNode GetUnderMouseUIGraphNode()
            {
                return Graph.Nodes.LastOrDefault(n => (n as UIGraphNode).UnderMouse) as UIGraphNode;
            }

            public UIGraphWay GetUnderMouseUIGraphWay()
            {
                return Graph.Ways.LastOrDefault(w => (w as UIGraphWay).UnderMouse) as UIGraphWay;
            }

            public GraphCanvas()
                : base()
            {
                Graph = new UIGraph();
            }
        }
    }
}
