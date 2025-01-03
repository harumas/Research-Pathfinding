using PathFinder.Core;

namespace PathFinding
{
    public class GraphConstructor
    {
        private readonly MapData mapData;
        private readonly GridGraphMediator mediator;

        public GraphConstructor(MapData mapData, GridGraphMediator mediator)
        {
            this.mapData = mapData;
            this.mediator = mediator;
        }

        private readonly Vector2Int[] direction = new[]
        {
            // 上下左右移動
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1),

            // 斜め移動
            // new Vector2Int(1, 1),
            // new Vector2Int(-1, -1),
            // new Vector2Int(-1, 1),
            // new Vector2Int(1, -1),
        };

        public Graph ConstructGraph()
        {
            Graph graph = new Graph(mapData.PassableCount);

            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    GridType from = mapData.Grids[y, x];

                    //障害物のバスならNodeを作らない
                    if (from == GridType.Obstacle)
                    {
                        continue;
                    }

                    //４方向にEdgeをつなぐ
                    foreach (Vector2Int dir in direction)
                    {
                        Vector2Int origin = new Vector2Int(x, y);
                        Vector2Int pos = origin + dir;
                        bool isConnectable = 0 <= pos.x && pos.x < mapData.Width && 0 <= pos.y && pos.y < mapData.Height;

                        if (!isConnectable)
                        {
                            continue;
                        }

                        GridType to = mapData.Grids[pos.y, pos.x];

                        if (to == GridType.Road)
                        {
                            graph.AddEdge(mediator.GetNode(origin), mediator.GetNode(pos));
                        }
                    }
                }
            }

            return graph;
        }
    }
}