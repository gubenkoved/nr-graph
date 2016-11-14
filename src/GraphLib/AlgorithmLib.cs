using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphLib
{
    /// <summary>
    /// Библиотека алгоритмов
    /// </summary>
    public static class AlgorithmLib
    {
        /// <summary>
        /// Коллекция алгоритмов
        /// </summary>
        public static List<Algorithm> Collection;
        /// <summary>
        /// Статический конструктор, который создаст единственную копию коллекции
        /// </summary>
        static AlgorithmLib()
        {
            Collection = new List<Algorithm>();

            Collection.Add(new Numerator());
            Collection.Add(new NameCleaner());
            Collection.Add(new Weigher());
            Collection.Add(new CalculatingComponentsConnectedness());
            Collection.Add(new Kruskal());
            Collection.Add(new Deikstra());
            Collection.Add(new MakeNonoriented());
            Collection.Add(new DFS());
            Collection.Add(new BFS());
            Collection.Add(new FundamentalCyclesFinder());
        }

    }
}
