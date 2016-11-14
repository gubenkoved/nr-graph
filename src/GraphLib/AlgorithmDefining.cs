using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace GraphLib
{
    public class AlgorithmException : Exception
    {
        public AlgorithmException(string message)
            : base(message)
        {
        }
    }    

    public class GraphWithText
    {
        public Graph Graph;
        public string Text;
    }

    #region Interfaces for 'decryptyng' result of algorithm work
    public interface IReturnGraphWithText
    {
        GraphWithText Decrypt(object obj);
    }

    public interface IReturnGraph
    {
        Graph Decrypt(object obj);
    }
    #endregion

    #region Algothims definition
    
    internal class CalculatingComponentsConnectedness
    : Algorithm, IReturnGraphWithText
    {
        // PRIVATE ACCESS

        private List<GraphNode> findListWithNode(List<List<GraphNode>> listNodesInList, GraphNode findNode)
        {
            foreach (List<GraphNode> list in listNodesInList)
            {
                if (list.Contains(findNode))
                    return list;
            }

            return null;
        }

        private void recourseWalker(Graph graph, List<GraphNode> listNodes, List<GraphNode> notMarked, GraphNode activeNode)
        {
            listNodes.Add(activeNode);
            notMarked.Remove(activeNode);

            foreach (GraphNode node in GraphHelper.GetIncidentNodes(graph, activeNode, false))
            {
                if (notMarked.Contains(node))
                {
                    recourseWalker(graph, listNodes, notMarked, node);
                }
            }
        }

        internal int Calculate(Graph graph, bool addGroupNumbers = false)
        {
            var listNodesInList = new List<List<GraphNode>>();    // лист компонент связанности

            var notMarked = new List<GraphNode>(graph.Nodes);

            while (notMarked.Count != 0)
            {
                List<GraphNode> list = new List<GraphNode>();

                recourseWalker(graph, list, notMarked, notMarked[0]);  // заполняем лист

                listNodesInList.Add(list);              // добовляем компоненту связанности
            }

            if (addGroupNumbers)
            {
                for (int i = 0; i < listNodesInList.Count; i++)
                {
                    for (int j = 0; j < listNodesInList[i].Count; j++)
                    {
                        listNodesInList[i][j].Name = (i + 1).ToString(@"\##");
                    }
                }
            }

            return listNodesInList.Count;
        }

        // PUBLIC ACCESS

        public CalculatingComponentsConnectedness()
            : base("Подсчёт компонент связанности", "Алгоритм подсчитывает число компонент связанности", "Алгоритм использует рекурсивный алгоритм поиска в глубину для того, чтобы помечать все вершины достижимые из данной")
        {
        }

        public override object Execute(Graph graph)
        {
            return new GraphWithText()
            {
                Graph = graph,
                Text = string.Format("Число компонент связанности: {0:#;#;0 (граф пуст)}", Calculate(graph, true))
            };
        }

        public GraphWithText Decrypt(object obj)
        {
            return obj as GraphWithText;
        }
    }

    internal class Numerator
        : Algorithm, IReturnGraph
    {
        // PRIVATE ACCESS

        // PUBLIC ACCESS

        public Numerator()
            : base("Нумератор вершин", "Алгоритм нумерует вершины", "")
        {
        }

        public override object Execute(Graph graph)
        {
            int counter = 1;

            foreach (GraphNode node in graph.Nodes)
            {
                node.Name = (++counter).ToString("№#");
            }

            return graph;
        }

        public Graph Decrypt(object obj)
        {
            return obj as Graph;
        }
    }

    internal class NameCleaner
        : Algorithm, IReturnGraph
    {

        // PRIVATE ACCESS

        // PUBLIC ACCESS

        public NameCleaner()
            : base("Стереть метки у вершин", "Алгоритм убирает с вершин все метки", "")
        {
        }

        public override object Execute(Graph graph)
        {
            foreach (GraphNode node in graph.Nodes)
            {
                node.Name = "";
                node.Info = "";
            }

            return graph;
        }

        public Graph Decrypt(object obj)
        {
            return obj as Graph;
        }
    }

    internal class Weigher
        : Algorithm, IReturnGraphWithText
    {

        // PRIVATE ACCESS

        // PUBLIC ACCESS

        public Weigher()
            : base("Взвесить граф", "Алгоритм возвращает вес графа", "")
        {
        }

        public override object Execute(Graph graph)
        {
            double weight = 0;

            foreach (GraphWay way in graph.Ways)
            {
                weight += way.Weight;
            }

            return new GraphWithText()
            {
                Graph = graph,
                Text = weight.ToString("Вес графа: 0.0")
            };
        }

        public GraphWithText Decrypt(object obj)
        {
            return obj as GraphWithText;
        }
    }

    internal class Kruskal
        : Algorithm, IReturnGraph
    {
        // PRIVATE ACCESS

        private List<GraphNode> FindListWithNode(List<List<GraphNode>> listNodesInList, GraphNode findNode)
        {
            foreach (List<GraphNode> list in listNodesInList)
            {
                if (list.Contains(findNode))
                    return list;
            }

            return null;
        }

        private bool InOneComponentsConnectedness(GraphNode a, GraphNode b, List<List<GraphNode>> listNodesInList)
        {
            List<GraphNode> aList = FindListWithNode(listNodesInList, a);
            List<GraphNode> bList = FindListWithNode(listNodesInList, b);

            if (aList == null && bList == null)
            {
                List<GraphNode> newList = new List<GraphNode>();

                newList.Add(a);
                newList.Add(b);

                listNodesInList.Add(newList);

                return false;
            }
            else if (aList == bList)
            {
                return true;
            }
            else if (aList == null && bList != null)
            {
                bList.Add(a);

                return false;
            }
            else if (aList != null && bList == null)
            {
                aList.Add(b);

                return false;
            }
            else if (aList != bList && aList != null && bList != null)
            {
                aList.AddRange(bList);
                listNodesInList.Remove(bList);

                return false;
            }
            else
            {
                throw new Exception("Unexpected");
            }

        }

        // PUBLIC ACCESS

        public Kruskal()
            : base("Алгоритм Крускала", "Алгоритм строит минимальное по весу остовное дерево — ациклический связный подграф данного графа, в который входят все его вершины", "Ассимптотическая сложность алгоритма O(n∙log(n))")
        {
        }

        public override object Execute(Graph graph)
        {
            ValidateGraph(graph);

            var sortedWays = new List<GraphWay>(graph.Ways)
                .OrderBy(w => w.Weight).ToList();

            // delete all ways
            graph.RemoveWay(w => true);

            var listNodesInList = new List<List<GraphNode>>();    // лист компонент связанности

            foreach (GraphWay way in sortedWays)
            {
                if (!InOneComponentsConnectedness(way.From, way.To, listNodesInList))
                {
                    graph.AddWay(way);
                }
            }

            return graph;
        }

        public override void ValidateGraph(Graph graph)
        {
            #region Cheking for graph is non-oriented
            if (graph.IsOriented())
            {
                throw new AlgorithmException("Для работы алгоритма необходимо чтобы граф был неориентированным");
            }
            #endregion

            #region Checking for graph has one component connectedness
            var calc = new CalculatingComponentsConnectedness();

            if (calc.Calculate(graph) > 1)
                throw new AlgorithmException("Для работы алгоритма необходимо чтобы граф был связанным");
            #endregion
        }

        public Graph Decrypt(object obj)
        {
            return obj as Graph;
        }
    }

    internal class Deikstra
        : Algorithm, IReturnGraph
    {
        // PRIVATE ACCESS

        private class NodeInfo
        {
            public GraphNode Previous;
            
            public double Weight;
            
            public bool Visited;
           
            public NodeInfo()
            {
                Weight = double.MaxValue;
            }
        }

        private KeyValuePair<GraphNode, NodeInfo> findPairWithMinUnvisitedNode(Dictionary<GraphNode, NodeInfo> dictionary)
        {
            var min = new KeyValuePair<GraphNode, NodeInfo>();
            bool isFirstPair = true;

            foreach (var nodeWithInfo in dictionary)
            {
                if (!isFirstPair)
                {
                    if (nodeWithInfo.Value.Weight < min.Value.Weight && nodeWithInfo.Value.Visited == false)
                        min = nodeWithInfo;
                }
                else
                {
                    if (nodeWithInfo.Value.Visited == false)
                    {
                        isFirstPair = false;
                        min = nodeWithInfo;
                    }
                }
            }

            return min;
        }

        private List<GraphNode> getUnvisitedIncidentNodes(Graph graph, GraphNode node, Dictionary<GraphNode, NodeInfo> dictionary)
        {
            return GraphHelper.GetIncidentNodes(graph, node, true).Where<GraphNode>
                (n => !dictionary[n].Visited).ToList();
        }

        private List<GraphNode> createAnswer(GraphNode lastNode, Dictionary<GraphNode, NodeInfo> dictionary)
        {
            var result = new List<GraphNode>();

            GraphNode active = lastNode;

            while (dictionary[active].Previous != null)
            {
                result.Insert(0, active);

                active = dictionary[active].Previous;
            }

            result.Insert(0, active);

            return result;
        }

        // PUBLIC ACCESS

        public Deikstra()
            : base("Алгоритм Дейкстры", "Алгоритм ищет кратчайшие пути от заданной вершины до всех остальных", "Ассимптотическая сложность алгоритма O(n²)")
        {
        }

        public override object Execute(Graph graph)
        {
            ValidateGraph(graph);                

            GraphNode from = graph.MarkedNodes[0];
            GraphNode to = graph.MarkedNodes[1];

            #region Заполяем таблицу соответствий
            var dictionary = new Dictionary<GraphNode, NodeInfo>();   // таблица соответсвтий вершина-информация
            
            foreach (GraphNode node in graph.Nodes)
            {
                dictionary[node] = new NodeInfo();
            }

            dictionary[from].Weight = 0.0;
            #endregion

            #region Основной цикл алгоритма
            KeyValuePair<GraphNode, NodeInfo> active;

            while (true)
            {
                active = findPairWithMinUnvisitedNode(dictionary);

                if (active.Key == null)
                    break;

                active.Value.Visited = true;

                List<GraphNode> incidentUnvisitedNodes = getUnvisitedIncidentNodes(graph, active.Key, dictionary);

                foreach (GraphNode node in incidentUnvisitedNodes)
                {
                    GraphWay way = graph.FindWay(active.Key, node);

                    if (dictionary[node].Weight > active.Value.Weight + way.Weight)
                    {
                        dictionary[node].Weight = active.Value.Weight + way.Weight;
                        dictionary[node].Previous = active.Key;
                    }
                }
            }
            #endregion

            if (dictionary[to].Weight == double.MaxValue)
                throw new AlgorithmException("Конечная вершина недостижима из начальной");

            #region Изменяем граф в соответствии с результатами

            var cleaner = new NameCleaner();
            cleaner.Execute(graph);
            
            var answer = createAnswer(to, dictionary);

            graph.UnmarkAllWays();

            for (int i = 0; i < answer.Count; i++)
            {
                answer[i].Name = (i + 1).ToString(@"\##");

                if (i != 0)
                {
                    graph.FindWay(answer[i - 1], answer[i]).Style = WayStyle.Marked;
                }
            }

            foreach (var pair in dictionary)
            {
                if (pair.Value.Weight != double.MaxValue)
                    pair.Key.Info = pair.Value.Weight.ToString("0.0");
                else
                    pair.Key.Info = "N/A";
            }

            #endregion

            return graph;
        }

        public override void ValidateGraph(Graph graph)
        {
            if (graph.MarkedNodes.Count != 2)
                throw new AlgorithmException("Для работы алгоритма требуется выделить 2 вершины");
        }

        public Graph Decrypt(object obj)
        {
            return obj as Graph;
        }
    }

    internal class MakeNonoriented
        : Algorithm, IReturnGraph
    {
        // PRIVATE ACCESS

        // PUBLIC ACCESS

        public MakeNonoriented()
            : base("Сделать неориентированный граф", "Превращает все ориентированные пути в неориентированные","")
        {
        }

        public override object Execute(Graph graph)
        {
            foreach (var way in graph.Ways)
            {
                way.Oriented = false;
            }

            return graph;
        }

        public Graph Decrypt(object obj)
        {
            return obj as Graph;
        }
    } 

    internal class DFS
        : Algorithm, IReturnGraph
    {
        // PRIVATE ACCESS

        void recourseWalker(Graph graph, GraphNode previous, GraphNode current, HashSet<GraphNode> visited)
        {
            current.Name = (visited.Count + 1).ToString(@"\##");

            visited.Add(current);

            if (previous != null)
            {
                graph.FindWay(previous, current, true).Style = WayStyle.Marked;
            }

            //recourseWalker( graph, current, 
            foreach (var node in GraphHelper.GetIncidentNodes(graph, current))
            {
                if (!visited.Contains(node))
                {
                    recourseWalker(graph, current, node, visited);
                }
            }
        }

        // PUBLIC ACCESS

        public DFS()
            : base("Обход в глубину", "Поиск в глубину (англ. Depth-first search, DFS) — один из методов обхода графа. Алгоритм поиска описывается следующим образом: для каждой непройденной вершины необходимо найти все не пройденные смежные вершины и повторить поиск для них.", "")
        {
        }

        public override object Execute(Graph graph)
        {
            ValidateGraph(graph);

            var cleaner = new NameCleaner();
            cleaner.Execute(graph);

            graph.UnmarkAllWays();

            var visited = new HashSet<GraphNode>();

            recourseWalker(graph, null, graph.MarkedNodes[0], visited);

            return graph;
        }

        public override void ValidateGraph(Graph graph)
        {
            if (graph.MarkedNodes.Count != 1)
                throw new AlgorithmException("Для работы алгоритма требуется выделить 1 вершину");
        }

        public Graph Decrypt(object obj)
        {
            return obj as Graph;
        }
    }

    internal class BFS
        : Algorithm, IReturnGraph
    {
        // PRIVATE ACCESS

        private void recourseWalker(Graph graph, GraphNode current, Queue<GraphNode> queue, HashSet<GraphNode> visited, Dictionary<GraphNode, int> distnaces)
        {
            if (visited.Contains(current))
                return;

            visited.Add(current);

            current.Name = visited.Count.ToString(@"\#0");
            current.Info = distnaces[current].ToString(@"0");

            foreach (var node in GraphHelper.GetIncidentNodes(graph, current, true))
            {
                if (!visited.Contains(node) && !queue.Contains(node))
                {
                    distnaces[node] = distnaces[current] + 1;

                    graph.FindWay(current, node, true).Style = WayStyle.Marked;

                    queue.Enqueue(node);
                }
            }

            while (queue.Count != 0)
            {
                recourseWalker(graph, queue.Dequeue(), queue, visited, distnaces);
            }
        }

        // PUBLIC ACCESS

        public BFS()
            : base("Обход в ширину", "Поиск в ширину (BFS, Breadth-first search) — метод обхода и разметки вершин графа. Поиск в ширину выполняется в следующем порядке: началу обхода s приписывается метка 0, смежным с ней вершинам — метка 1. Затем поочередно рассматривается окружение всех вершин с метками 1, и каждой из входящих в эти окружения вершин приписываем метку 2 и т. д.", "")
        {
        }

        public override object Execute(Graph graph)
        {
            ValidateGraph(graph);
            
            var cleaner = new NameCleaner();
            cleaner.Execute(graph);

            graph.UnmarkAllWays();

            var initNode = graph.MarkedNodes[0];
            var queue = new Queue<GraphNode>();
            var visited = new HashSet<GraphNode>();
            var distances = new Dictionary<GraphNode, int>();

            distances[initNode] = 0;

            recourseWalker(graph, initNode, queue, visited, distances);

            return graph;
        }

        public override void ValidateGraph(Graph graph)
        {
            if (graph.MarkedNodes.Count != 1)
                throw new AlgorithmException("Для работы алгоритма требуется выделить 1 вершину");
        }

        public Graph Decrypt(object obj)
        {
            return obj as Graph;
        }
    }

    internal class FundamentalCyclesFinder
        : Algorithm, IReturnGraphWithText
    {
        private bool skeletTreePathFinder(Graph skeletTree, GraphNode from, GraphNode current, GraphNode to, ref List<GraphNode> path)
        {
            if (current == to)
            {
                path.Add(current);
                return true;
            }

            var incedentNodes = GraphHelper.GetIncidentNodes(skeletTree, current);
            incedentNodes.Remove(from);

            foreach (var incedentNode in incedentNodes)
            {
                if (skeletTreePathFinder(skeletTree, current, incedentNode, to, ref path) == true)
                {
                    path.Add(current);
                    return true;
                }
            }

            return false;
        }

        public FundamentalCyclesFinder()
            :base("Поиск фундаментального множества циклов", "", "" )
        {
        }

        public override object Execute(Graph graph)
        {
            ValidateGraph(graph);

            var numerator = new Numerator();
            numerator.Execute(graph);

            var graphClone = graph.Clone();

            // кодерево -  множество всех рёбер графа, кроме остовных
            var coTree = new List<GraphWay>(graphClone.Ways);

            var skeletTreeGenerator = new Kruskal();
            var skeletTree = skeletTreeGenerator.Decrypt(skeletTreeGenerator.Execute(graphClone));
            
            coTree.RemoveAll(w => skeletTree.Ways.Contains(w));

            var cycles = new List<List<GraphNode>>();

            foreach (var coTreeWay in coTree)
            {
                var from = coTreeWay.From;
                var to = coTreeWay.To;

                var cycle = new List<GraphNode>();
                cycles.Add(cycle);

                skeletTreePathFinder(skeletTree, from, from, to, ref cycle);
            }

            string textResult = "";

            foreach (var cycle in cycles)
            {
                for (int i = 0; i < cycle.Count; i++)
                    textResult += cycle[i].Name + " → ";

                textResult += cycle[0].Name + "\n";
            }

            textResult += cycles.Count.ToString("Всего циклов: 0");

            return new GraphWithText() { Graph = graph, Text = textResult };
        }

        public override void ValidateGraph(Graph graph)
        {
            #region Cheking for graph is non-oriented
            if (graph.IsOriented())
            {
                throw new AlgorithmException("Для работы алгоритма необходимо чтобы граф был неориентированным");
            }
            #endregion

            #region Checking for graph has one component connectedness
            var calc = new CalculatingComponentsConnectedness();

            if (calc.Calculate(graph) > 1)
                throw new AlgorithmException("Для работы алгоритма необходимо чтобы граф был связанным");
            #endregion
        }

        public GraphWithText Decrypt(object obj)
        {
            return obj as GraphWithText;
        }
    }
    
    #endregion

}
