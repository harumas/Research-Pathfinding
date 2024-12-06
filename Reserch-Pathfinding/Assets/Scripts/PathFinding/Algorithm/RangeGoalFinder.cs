using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;
using UnityEngine;
using Vector2Int = PathFinder.Core.Vector2Int;

namespace PathFinding.Algorithm
{
    public class RangeGoalFinder
    {
        private readonly Graph graph;
        private readonly GridGraphMediator mediator;

        public RangeGoalFinder(Graph graph, GridGraphMediator mediator)
        {
            this.graph = graph;
            this.mediator = mediator;
        }

        /// <summary>
        /// 範囲ゴールを探索します
        /// </summary>
        public List<HashSet<int>> GetRangeGoals(Vector2Int center, int radius)
        {
            var rangeGoals = new List<HashSet<int>>();

            //円周上のグリッドを取得する
            var circleNodes = CalculateCircleNodes(center, radius);

            while (circleNodes.Count > 0)
            {
                //範囲ゴールを追加
                int first = circleNodes.First();
                var rangeGoal = new HashSet<int>() { first };
                rangeGoals.Add(rangeGoal);

                Queue<int> open = new Queue<int>();
                open.Enqueue(first);

                //繋がっているグリッドを探索する
                while (open.Count > 0)
                {
                    int current = open.Dequeue();
                    circleNodes.Remove(current);

                    foreach (int neighbor in graph.GetNextNodes(current))
                    {
                        if (circleNodes.Contains(neighbor))
                        {
                            rangeGoal.Add(neighbor);
                            open.Enqueue(neighbor);
                        }
                    }
                }
            }

            return rangeGoals;
        }

        private HashSet<int> CalculateCircleNodes(Vector2Int center, int radius)
        {
            HashSet<int> circlePoints = new HashSet<int>();

            Vector2Int pos = new Vector2Int(0, 0);
            int d = 0;

            d = 3 - 2 * radius;
            pos.y = radius;

            SetCirclePoint(circlePoints, center.x, radius + center.y);
            SetCirclePoint(circlePoints, center.x, -radius + center.y);
            SetCirclePoint(circlePoints, radius + center.x, center.y);
            SetCirclePoint(circlePoints, -radius + center.x, center.y);

            Vector2Int previous = new Vector2Int();

            for (pos.x = 0; pos.x <= pos.y; pos.x++)
            {
                if (d < 0)
                {
                    d += 6 + 4 * pos.x;
                }
                else
                {
                    d += 10 + 4 * pos.x - 4 * pos.y--;
                }

                Vector2Int current = new Vector2Int(pos.y + center.x, pos.x + center.y);
                SetCirclePoint(circlePoints, pos.y + center.x, pos.x + center.y);
                SetCirclePoint(circlePoints, pos.x + center.x, pos.y + center.y);
                SetCirclePoint(circlePoints, -pos.x + center.x, pos.y + center.y);
                SetCirclePoint(circlePoints, -pos.y + center.x, pos.x + center.y);
                SetCirclePoint(circlePoints, -pos.y + center.x, -pos.x + center.y);
                SetCirclePoint(circlePoints, -pos.x + center.x, -pos.y + center.y);
                SetCirclePoint(circlePoints, pos.x + center.x, -pos.y + center.y);
                SetCirclePoint(circlePoints, pos.y + center.x, -pos.x + center.y);

                if (pos.x > 0)
                {
                    Vector2Int dv = previous - current;
                    if (dv == new Vector2Int(1, -1))
                    {
                        Vector2Int p = previous + new Vector2Int(0, 1) - center;
                        SetCirclePoint(circlePoints, p.x + center.x, p.y + center.y);
                        SetCirclePoint(circlePoints, p.y + center.x, p.x + center.y);
                        SetCirclePoint(circlePoints, -p.x + center.x, p.y + center.y);
                        SetCirclePoint(circlePoints, -p.y + center.x, p.x + center.y);
                        SetCirclePoint(circlePoints, -p.x + center.x, -p.y + center.y);
                        SetCirclePoint(circlePoints, -p.y + center.x, -p.x + center.y);
                        SetCirclePoint(circlePoints, p.x + center.x, -p.y + center.y);
                        SetCirclePoint(circlePoints, p.y + center.x, -p.x + center.y);
                    }
                }

                previous = current;
            }

            return circlePoints;
        }

        private void SetCirclePoint(HashSet<int> circlePoints, int x, int y)
        {
            var pos = new Vector2Int(x, y);

            if (mediator.TryGetNode(pos, out int node))
            {
                circlePoints.Add(node);
            }
        }
    }
}