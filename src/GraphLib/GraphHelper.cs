using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphLib
{
    /// <summary>
    /// Contains main and simple algorithms with graph
    /// </summary>
    internal static class GraphHelper
    {
        /// <summary>
        /// Geting incident nodes
        /// </summary>
        /// <param name="graph">Initial graph for searching incident nodes</param>
        /// <param name="node">Current node</param>
        /// <param name="lookOnDirection">If this parameter is false, then the vertex is incident being just adjacent to the initial vertex</param>
        /// <returns>List of incident vertexes</returns>
        public static List<GraphNode> GetIncidentNodes(Graph graph, GraphNode node, bool lookOnDirection = true)
        {
            var result = new List<GraphNode>();

            foreach (var way in graph.Ways)
            {
                if (way.Has(node))
                {
                    if (!lookOnDirection || !way.Oriented || way.From == node)
                    {
                        result.Add(way.Another(node));
                    }
                }
                    
            }

            return result;
        }
    }
}
