using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class Graph
    {
        private readonly Dictionary<int, List<int>> graph;

        public int NodeCount => graph.Count;

        public Graph(int capacity)
        {
            graph = new Dictionary<int, List<int>>(capacity);
        }

        public void AddEdge(int from, int to)
        {
            if (!graph.ContainsKey(from))
            {
                //graph[from] = new List<int>(3);
                graph[from] = new List<int>(3);
            }
            
            graph[from].Add(to);
        }

        public IReadOnlyCollection<int> GetNextNodes(int node)
        {
            return graph[node];
        }
    }
}