using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphLib
{
    /// <summary>
    /// Абстрактный алгоритм
    /// </summary>
    public abstract class Algorithm
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public Algorithm(string AlgorithmName, string AlgorithmInfo, string AlgorithmNote)
        {
            Name = AlgorithmName;
            Info = AlgorithmInfo;
            Note = AlgorithmNote;
        }
        /// <summary>
        /// Выполнить алгоритм
        /// </summary>
        /// <param name="initialGraph">Граф, над которым будет производиться операция</param>
        /// <param name="parameters">Параметры алгоритма</param>
        public abstract object Execute(Graph graph);
        /// <summary>
        /// Проверка валидности графа
        /// </summary>
        public virtual void ValidateGraph(Graph graph)
        {           
        }
        /// <summary>
        /// Перегрузка метода Object.ToString()
        /// </summary>
        public override string ToString()
        {
            //return Name;
            return base.ToString();
        }

        /// <summary>
        /// Имя алгоритма
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Информация о алгоритме
        /// </summary>
        public string Info { get; private set; }
        /// <summary>
        /// Примечания к алгоритму
        /// </summary>
        public string Note { get; private set; }
    }
}
