using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding.Algorithm
{
    public class RangeGoalAlgorithmWithBFS : ISolver
    {
        private readonly GridGraphMediator mediator;
        private readonly ConstrainedAStar pathFinder;
        private readonly BFS bfs;
        private readonly RangeGoalFinder rangeGoalFinder;

        public HashSet<int> CorrectGoals;
        public HashSet<int> IncorrectGoals;

        public RangeGoalAlgorithmWithBFS(Graph graph, GridGraphMediator mediator)
        {
            this.mediator = mediator;

            pathFinder = new ConstrainedAStar(graph, CreateNodes(graph));
            bfs = new BFS(graph, CreateNodes(graph));
            rangeGoalFinder = new RangeGoalFinder(graph, mediator);
        }

        public List<int> Solve(int start, int goal)
        {
            Vector2Int startPos = mediator.GetPos(start);
            Vector2Int goalPos = mediator.GetPos(goal);

            const float alpha = 0.5f;
            Vector2Int diff = startPos - goalPos;

            // ゴールからスタートの距離 * alphaの長さをゴールの範囲とする
            double distance = Math.Sqrt(diff.x * diff.x + diff.y * diff.y) * alpha;
            int radius = (int)Math.Round(distance);

            // 範囲ゴールを取得
            List<HashSet<int>> rangeGoals = rangeGoalFinder.GetRangeGoals(goalPos, radius);

            // 到達可能な範囲ゴール
            HashSet<int> correctRangeGoal = bfs.FindReachableGoals(goal, rangeGoals.SelectMany(item => item).ToHashSet(), radius);
            CorrectGoals = correctRangeGoal;

            HashSet<int> rangeGoalSet = rangeGoals.SelectMany(item => item).ToHashSet();
            rangeGoalSet.ExceptWith(correctRangeGoal);
            IncorrectGoals = rangeGoalSet;

            // 到達可能な範囲ゴールに対して経路探索
            List<Node> result = pathFinder.FindPath(start, correctRangeGoal, mediator.GetPos(goal));

            // ノードに変換して結果に追加
            return result.Select(node => node.Index).ToList();
        }

        private List<Node> CreateNodes(Graph graph)
        {
            // 座標からアルゴリズムで使用するノードを作成
            List<Node> nodes = new List<Node>(graph.NodeCount);

            for (int i = 0; i < graph.NodeCount; i++)
            {
                Vector2Int pos = mediator.GetPos(i);
                nodes.Add(new Node(i, new Vector2(pos.x, pos.y)));
            }

            return nodes;
        }
    }
}