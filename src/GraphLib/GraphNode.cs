using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;


namespace GraphLib
{
    public enum NodeStyle { Simple, Selected, Marked }
    public struct NodeStyleChangedArg
    {
        public GraphNode Node;
        public NodeStyle OldStyle;
        public NodeStyle NewStyle;
    }

    /// <summary>
    /// Представляет вершину графа
    /// </summary>
    [Serializable]
    public class GraphNode
    {
        // PRIVATE ACCESS
        
        private Point position;
        
        private NodeStyle style;

        // PUBLIC ACCESS

        /// <summary>
        /// Конструктор
        /// </summary>
        public GraphNode( Point NodePosition, string NodeName = "", string NodeInfo = "", NodeStyle Style = NodeStyle.Simple )
        {
            Position = NodePosition;            
            Name = NodeName;
            Info = NodeInfo;
            style = Style;
        }

        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        
        public string Info { get; set; }

        /// <summary>
        /// Позиция вершины графа
        /// </summary>
        public Point Position
        {
            get { return position; }

            set { position = value; }
        }
        
        /// <summary>
        /// Стиль вершины
        /// </summary>
        public NodeStyle Style
        {
            get { return style; }

            set 
            {
                var oldStyle = style;

                style = value;

                if (NodeStyleChanged != null)
                {
                    NodeStyleChanged(new NodeStyleChangedArg() { Node = this, OldStyle = oldStyle, NewStyle = style });
                }
            }
        }

        public delegate void NodeStyleChangedHandler(NodeStyleChangedArg arg);
        public event NodeStyleChangedHandler NodeStyleChanged;
    }
}
