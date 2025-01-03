using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding.Algorithm
{
    public class BFS
    {
        private readonly Graph graph;
        private readonly List<Node> nodes;

        public BFS(Graph graph, List<Node> nodes)
        {
            this.graph = graph;
            this.nodes = nodes;
        }

        public HashSet<int> FindReachableGoals(
            int start,
            HashSet<int> end,
            float radius
        )
        {
            ResetNodes();
            var openList = new Queue<Node>();
            var closedList = new HashSet<int>();
            var result = new HashSet<int>();

            Node startNode = nodes[start];

            //初期ノードの作成
            startNode.Time = 0;
            openList.Enqueue(startNode);
            closedList.Add(startNode.Index);

            float detectRadius = (radius + 0.5f) * (radius + 0.5f);

            // 待機し続けた時に打ち切る
            while (openList.Count > 0)
            {
                Node node = openList.Dequeue();
                List<int> nextNodes = graph.GetNextNodes(node.Index).ToList();

                foreach (int neighbourIndex in nextNodes)
                {
                    if (!closedList.Add(neighbourIndex))
                    {
                        continue;
                    }

                    Node neighbour = nodes[neighbourIndex];
                    Vector2 diff = neighbour.Position - startNode.Position;

                    if (diff.x * diff.x + diff.y * diff.y > detectRadius)
                    {
                        continue;
                    }

                    openList.Enqueue(neighbour);

                    if (end.Contains(neighbourIndex))
                    {
                        result.Add(neighbourIndex);
                    }
                }
            }

            //パスを見つけられなかったら0
            return result;
        }

        private void ResetNodes()
        {
            foreach (Node node in nodes)
            {
                node.Reset();
            }
        }
    }
}