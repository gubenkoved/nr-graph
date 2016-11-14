using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphLib
{
    /// <summary>
    /// Представляет путь в графе
    /// </summary>
    public enum WayStyle { Simple, Selected, Marked }

    [Serializable]
    public class GraphWay
    {
        // PRIVATE ACCESS

        private double weight;

        // PROTECTED ACCESS

        protected GraphNode from;

        protected GraphNode to;

        protected WayStyle style;

        // PUBLIC ACCESS

        /// <summary>
        /// Конструктор
        /// </summary>
        public GraphWay(GraphNode FromNode, GraphNode ToNode, bool oriented)
        {
            from = FromNode;
            to = ToNode;
            style = WayStyle.Simple;
            
            WeightIsLen = true;
            Oriented = oriented;
        }
        
        public GraphWay(GraphNode FromNode, GraphNode ToNode, double WayWeight, bool oriented)
            : this( FromNode, ToNode, oriented )
        {
            weight = WayWeight;
            WeightIsLen = false;
        }

        /// <summary>
        /// Стиль
        /// </summary>
        public WayStyle Style
        {
            get
            {
                return style;
            }
            set
            {
                style = value;
            }
        }
       
        public GraphNode From
        {
            get
            {
                return from;
            }
        }
        
        public GraphNode To
        {
            get
            {
                return to;
            }
        }
        
        public bool Oriented { get; set; }
        
        public bool WeightIsLen { get; set; }
        
        public double Weight
        {
            get
            {
                if (WeightIsLen)
                    return Math.Sqrt(Math.Pow((from.Position.X - to.Position.X), 2) + Math.Pow((from.Position.Y - to.Position.Y), 2));
                else
                    return weight;
            }
            set
            {
                weight = value;
            }
        }

        public bool Has(GraphNode node)
        {
            return node == From || node == To;
        }
        
        public bool Has(GraphNode node1, GraphNode node2)
        {
            return node1 == From && node2 == To || node1 == To && node2 == From;
        }
        
        public GraphNode Another(GraphNode one)
        {
            if (Has(one))
            {
                if (From == one)
                    return To;
                else
                    return From;
            }
            else
                return null;
        }
    }
}
