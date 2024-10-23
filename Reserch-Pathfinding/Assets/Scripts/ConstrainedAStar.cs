using System.Collections.Generic;
using System.Linq;
using Core;

namespace PathFinding.Algorithm
{
    /// <summary>
    /// 制約を考慮するA*アルゴリズム
    /// </summary>
    public class ConstrainedAStar
    {
        private readonly Graph graph;
        private readonly Dictionary<int, Node> nodes;

        public ConstrainedAStar(Graph graph, Dictionary<int, Node> nodes)
        {
            this.graph = graph;
            this.nodes = nodes;
        }

        public List<Node> FindPath(
            int start,
            int end
        )
        {
            ResetNodes();
            var openList = new PriorityQueue<float, (float f, Node node)>(item => item.f, false);
            var closedList = new HashSet<int>();

            Node startNode = nodes[start];
            Node targetNode = nodes[end];

            //初期ノードの作成
            startNode.Distance = 0;
            startNode.H = Heuristic(startNode, targetNode);
            openList.Enqueue((0, startNode));
            closedList.Add(startNode.Index);

            // 待機し続けた時に打ち切る
            int timeout = graph.NodeCount;

            while (openList.Count > 0)
            {
                Node node = openList.Dequeue().node;

                //ゴールに到達したら
                if (node.Index == targetNode.Index)
                {
                    //親まで辿ってパスを返す
                    var r = RetracePath(node);

                    return r;
                }

                if (timeout < node.Distance)
                {
                    continue;
                }

                foreach (int neighbourIndex in graph.GetNextNodes(node.Index))
                {
                    if (!closedList.Add(neighbourIndex))
                    {
                        continue;
                    }

                    Node neighbour = nodes[neighbourIndex];

                    neighbour.H = Heuristic(neighbour, targetNode);
                    neighbour.Distance = node.Distance + Heuristic(node, neighbour);
                    neighbour.Parent = node;

                    openList.Enqueue((neighbour.F, neighbour));
                }
            }

            //パスを見つけられなかったらnull
            return null;
        }

        private float Heuristic(Node nodeA, Node nodeB)
        {
            Vector2 ab = nodeA.Position - nodeB.Position;
            float magnitude = ab.x * ab.x + ab.y * ab.y;
            return magnitude;
        }

        private List<Node> RetracePath(Node current)
        {
            var path = new List<Node>();

            while (current != null)
            {
                path.Add(current);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        private void ResetNodes()
        {
            foreach (Node node in nodes.Values)
            {
                node.Reset();
            }
        }
    }
}