using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding
{
    /// <summary>
    /// シーン上のグリッドとグラフを仲介するクラス
    /// </summary>
    public class GridGraphMediator
    {
        private readonly MapData mapData;
        private readonly Dictionary<int, int> nodeIndexList;
        private readonly Dictionary<int, int> indexNodeList;

        public GridGraphMediator(MapData mapData)
        {
            this.mapData = mapData;
            nodeIndexList = CreateNodeIndexList();
            indexNodeList = nodeIndexList.ToDictionary(x => x.Value, x => x.Key);
        }

        /// <summary>
        /// 2次元座標からノードインデックスを取得します
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int GetNode(Vector2Int pos)
        {
            int index = pos.y * mapData.Width + pos.x;
            return indexNodeList[index];
        }


        public bool TryGetNode(Vector2Int pos, out int node)
        {
            if (pos.x < 0 || pos.x >= mapData.Width || pos.y < 0 || pos.y >= mapData.Height)
            {
                node = -1;
                return false;
            }

            if ((mapData.Grids[pos.y, pos.x] & GridType.Road) != 0)
            {
                node = GetNode(pos);
                return true;
            }

            node = -1;
            return false;
        }

        /// <summary>
        /// ノードインデックスから2次元座標を取得します
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Vector2Int GetPos(int node)
        {
            int index = nodeIndexList[node];
            return new Vector2Int(index % mapData.Width, index / mapData.Width);
        }

        private Dictionary<int, int> CreateNodeIndexList()
        {
            var list = new Dictionary<int, int>();

            int nodeCount = 0;
            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    GridType gridType = mapData.Grids[y, x];

                    if (gridType == GridType.Road)
                    {
                        list.Add(nodeCount++, y * mapData.Width + x);
                    }
                }
            }

            return list;
        }
    }
}