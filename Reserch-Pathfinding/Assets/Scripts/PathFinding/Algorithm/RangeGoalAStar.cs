using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding.Algorithm
{
    public class RangeGoalAStar
    {
        private readonly Graph graph;
        private readonly List<Node> nodes;
        private readonly HashSet<int> passableNodes;

        public RangeGoalAStar(Graph graph, List<Node> nodes)
        {
            this.graph = graph;
            this.nodes = nodes;

            passableNodes = new HashSet<int>();
        }

        public void Reset()
        {
            passableNodes.Clear();
        }

        public List<Node> FindReachablePath(
            int start,
            int end,
            float radius
        )
        {
            ResetNodes();
            var openList = new PriorityQueue<float, (float f, Node node)>(item => item.f, false);
            var closedList = new HashSet<int>();

            Node startNode = nodes[start];
            Node endNode = nodes[end];

            //初期ノードの作成
            startNode.Time = 0;
            startNode.H = Heuristic(startNode, endNode.Position);
            openList.Enqueue((0, startNode));
            closedList.Add(startNode.Index);

            // 待機し続けた時に打ち切る
            while (openList.Count > 0)
            {
                Node node = openList.Dequeue().node;

                //ゴールに到達したら
                if (end == node.Index || passableNodes.Contains(node.Index))
                {
                    //親まで辿ってパスを返す
                    var path = RetracePath(node);
                    foreach (Node n in path)
                    {
                        passableNodes.Add(n.Index);
                    }

                    return path;
                }

                //探索するグリッドが範囲外ならスキップ
                if (node.H > radius)
                {
                    continue;
                }

                List<int> nextNodes = graph.GetNextNodes(node.Index).ToList();

                foreach (int neighbourIndex in nextNodes)
                {
                    if (!closedList.Add(neighbourIndex))
                    {
                        continue;
                    }

                    Node neighbour = nodes[neighbourIndex];

                    neighbour.H = Heuristic(neighbour, endNode.Position);
                    neighbour.Time = node.Time + 1;
                    neighbour.Parent = node;

                    openList.Enqueue((neighbour.F, neighbour));
                }
            }
            
            return new List<Node>();
        }

        private float Heuristic(Node nodeA, Vector2 target)
        {
            Vector2 ab = nodeA.Position - target;
            float magnitude = (float)Math.Sqrt(ab.x * ab.x + ab.y * ab.y);
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
            foreach (Node node in nodes)
            {
                node.Reset();
            }
        }
    }
}