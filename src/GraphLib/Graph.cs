using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace GraphLib
{
    [Serializable]
    public class Graph
    {
        // PRIVATE ACCESS
        private List<GraphNode> nodes;

        private List<GraphWay> ways;

        private List<GraphNode> markedNodes;

        private void node_NodeStyleChanged(NodeStyleChangedArg arg)
        {
            if (arg.NewStyle == NodeStyle.Marked)
                markedNodes.Add(arg.Node);
            else if (markedNodes.Contains(arg.Node))
                markedNodes.Remove(arg.Node);
        }

        // PUBLIC ACCESS

        public void AddNode(GraphNode node)
        {
            nodes.Add(node);

            node.NodeStyleChanged += new GraphNode.NodeStyleChangedHandler(node_NodeStyleChanged);
        }

        public void AddWay(GraphWay way)
        {
            ways.Add(way);
        }

        public void RemoveNode(GraphNode node)
        {
            if (markedNodes.Contains(node))
                markedNodes.Remove(node);

            ways.RemoveAll(w => w.Has(node));

            if (nodes.Contains(node))
                nodes.Remove(node);
        }

        public void RemoveWay(GraphWay way)
        {
            if (ways.Contains(way))
                ways.Remove(way);
        }

        public void RemoveWay(Predicate<GraphWay> predicate)
        {
            ways.RemoveAll(predicate);
        }

        public void UnmarkAllWays()
        {
            foreach (var way in ways)
            {
                way.Style = WayStyle.Simple;
            }
        }

        public void Clear()
        {
            nodes.Clear();
            ways.Clear();
            markedNodes.Clear();
        }

        public ReadOnlyCollection<GraphNode> MarkedNodes
        {
            get
            {
                return markedNodes.AsReadOnly();
            }
        }

        public ReadOnlyCollection<GraphNode> Nodes
        {
            get { return nodes.AsReadOnly(); }
        }

        public ReadOnlyCollection<GraphWay> Ways
        {
            get { return ways.AsReadOnly(); }
        }

        public GraphWay FindWay(GraphNode first, GraphNode second, bool oriented = false)
        {
            foreach (var way in ways)
            {
                if (way.Has(first, second))
                {
                    if (!oriented || !way.Oriented || way.From == first)
                        return way;
                }
            }

            return null;
        }

        public bool IsOriented()
        {
            foreach (var way in ways)
            {
                if (way.Oriented)
                    return true;
            }

            return false;
        }

        public Graph()
        {
            nodes = new List<GraphNode>();
            ways = new List<GraphWay>();
            markedNodes = new List<GraphNode>();
        }

        public Graph Clone()
        {
            var nodeDictionary = new Dictionary<GraphNode, GraphNode>();

            var copyOfGraph = new Graph();

            foreach (GraphNode node in Nodes)
            {
                var copyOfNode = new GraphNode(node.Position, node.Name, node.Info, node.Style);
                nodeDictionary[node] = copyOfNode; // соответствие между старой и новой ссылкой

                copyOfGraph.AddNode(copyOfNode);
            }

            foreach (GraphNode markedNode in MarkedNodes)
            {
                copyOfGraph.markedNodes.Add(nodeDictionary[markedNode]);
            }

            foreach (GraphWay way in Ways)
            {
                var copyOfWay = new GraphWay(nodeDictionary[way.From], nodeDictionary[way.To], way.Weight, way.Oriented);
                copyOfWay.WeightIsLen = way.WeightIsLen;
                copyOfWay.Style = way.Style;

                copyOfGraph.AddWay(copyOfWay);
            }

            return copyOfGraph;
        }

    }
}
