using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphLib
{
    public class GraphHistory
    {

        // PRIVATE ACCESS

        private int currentIndex;

        private List<Graph> history;

        // PUBLIC ACCESS

        public GraphHistory()
        {
            history = new List<Graph>();
            currentIndex = -1;
        }

        public Graph HistoryUndo()
        {
            --currentIndex;
            return history[currentIndex];
        }

        public Graph HistoryRedo()
        {
            ++currentIndex;
            return history[currentIndex];
        }

        public void HistoryShot(Graph graph)
        {
            if (history.Count > currentIndex + 1)
                history.RemoveRange(currentIndex + 1, history.Count - (currentIndex + 1));

            ++currentIndex;
            history.Add(graph);
        }

        public bool CanUndo()
        {
            return (currentIndex > 0);
        }

        public bool CanRedo()
        {
            return ((currentIndex + 1) < history.Count);
        }

        public void Clear()
        {
            history.Clear();
            currentIndex = -1;
        }
    }
}
