using System.Collections.Generic;

namespace PathFinder.Core
{
    public class Graph
    {
        private readonly HashSet<int>[] graph;

        public int NodeCount => graph.Length;

        public Graph(int capacity)
        {
            graph = new HashSet<int>[capacity];

            for (var i = 0; i < graph.Length; i++)
            {
                graph[i] = new HashSet<int>(64);
            }
        }

        public void AddEdge(int from, int to)
        {
            graph[from].Add(to);
        }

        public IReadOnlyCollection<int> GetNextNodes(int node)
        {
            return graph[node];
        }
    }
}