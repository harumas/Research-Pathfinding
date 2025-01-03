using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding.Algorithm
{
    /// <summary>
    /// 制約を考慮するA*アルゴリズム
    /// </summary>
    public class ConstrainedAStar
    {
        private readonly Graph graph;
        private readonly List<Node> nodes;

        public ConstrainedAStar(Graph graph, List<Node> nodes)
        {
            this.graph = graph;
            this.nodes = nodes;
        }

        public List<Node> FindPath(
            int start,
            HashSet<int> end,
            Vector2 target
        )
        {
            ResetNodes();
            var openList = new PriorityQueue<float, (float f, Node node)>(item => item.f, false);
            var closedList = new HashSet<int>();

            Node startNode = nodes[start];

            //初期ノードの作成
            startNode.Time = 0;
            startNode.H = Heuristic(startNode, target);
            openList.Enqueue((0, startNode));
            closedList.Add(startNode.Index);

            // 待機し続けた時に打ち切る
            while (openList.Count > 0)
            {
                Node node = openList.Dequeue().node;

                //ゴールに到達したら
                if (end.Contains(node.Index))
                {
                    //親まで辿ってパスを返す
                    var r = RetracePath(node);

                    return r;
                }

                List<int> nextNodes = graph.GetNextNodes(node.Index).ToList();

                foreach (int neighbourIndex in nextNodes)
                {
                    if (!closedList.Add(neighbourIndex))
                    {
                        continue;
                    }

                    Node neighbour = nodes[neighbourIndex];

                    neighbour.H = Heuristic(neighbour, target);
                    neighbour.Time = node.Time + 1;
                    neighbour.Parent = node;

                    openList.Enqueue((neighbour.F, neighbour));
                }
            }

            //パスを見つけられなかったら0
            return new List<Node>();
        }


        public List<Node> FindPathWithRange(
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
                if (end == node.Index)
                {
                    //親まで辿ってパスを返す
                    var r = RetracePath(node);

                    return r;
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

            //パスを見つけられなかったら0
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