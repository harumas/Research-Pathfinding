using System;
using System.Collections.Generic;
using UnityEngine;

namespace Visualizer.MapEditor
{
    [Flags]
    public enum GridType
    {
        Road = 1,
        Obstacle = 2,
        Path = 4
    }

    public class M_MapData
    {
        public readonly int Height;
        public readonly int Width;
        public readonly int PassableCount;
        public readonly IReadOnlyList<Vector2Int> Agents;
        public readonly Vector2Int Goal;
        public readonly GridType[,] Grids;

        public M_MapData(int height, int width, int passableCount, IReadOnlyList<Vector2Int> agents, Vector2Int goal, GridType[,] grids)
        {
            Height = height;
            Width = width;
            PassableCount = passableCount;
            Agents = agents;
            Goal = goal;
            Grids = grids;
        }
    }

    public class M_MapDataManager : MonoBehaviour
    {
        [SerializeField] private int defaultHeight;
        [SerializeField] private int defaultWidth;
        [SerializeField] private MapSaveData mapData;

        private MapData currentMapData;
        public MapData CurrentMapData => currentMapData;

        public MapData Load()
        {
            GridType[,] mapIds;
            int width, height;

            if (mapData.Data.Length == 0)
            {
                mapIds = GetDefaultMapData(defaultHeight, defaultWidth);
                width = defaultWidth;
                height = defaultHeight;
                Save(mapIds);
            }
            else
            {
                mapIds = ParseMapData();
                height = mapIds.GetLength(0);
                width = mapIds.GetLength(1);
            }

            int passableCount = 0;

            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isPassable = mapIds[z, x] == GridType.Road;

                    if (isPassable)
                    {
                        passableCount++;
                    }
                }
            }

            currentMapData = new MapData(height, width, passableCount, mapData.EndPoints, mapData.Goal, mapIds);

            return currentMapData;
        }

        public void Save(GridType[,] mapIds)
        {
            string[] data = mapData.Data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            int width = data.Length != 0 ? data[0].Length : defaultWidth;
            int height = data.Length != 0 ? data.Length : defaultHeight;

            char[] str = new char[width * height + height];
            Array.Fill(str, '.');

            for (int i = 0; i < mapIds.GetLength(0); i++)
            {
                int index = 0;
                for (int j = 0; j < mapIds.GetLength(1); j++)
                {
                    GridType v = mapIds[i, j];
                    index = i * width + j + i;
                    str[index] = v == GridType.Road ? '.' : '*';
                }

                str[index + 1] = '\n';
            }

            string result = new string(str);
            mapData.SetData(result);
        }

        private GridType[,] ParseMapData()
        {
            //string[] data = mapData.Data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string[] data = CheckRoundObstacle( mapData.Data.Split('\n', StringSplitOptions.RemoveEmptyEntries));

            int width = data.Length != 0 ? data[0].Length : defaultWidth;
            int height = data.Length != 0 ? data.Length : defaultHeight;

            GridType[,] mapIds = new GridType[height, width];

            for (var i = 0; i < height; i++)
            {
                var h = data[i];

                for (var j = 0; j < width; j++)
                {
                    mapIds[i, j] = h[j] == '.' ? GridType.Road : GridType.Obstacle;
                    Debug.Log($"i:{i},j:{j}={mapIds[i, j]}");

                }
            }

            return mapIds;
        }

        private GridType[,] GetDefaultMapData(int height, int width)
        {
            var map = new GridType[height, width];

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    map[y, x] = GridType.Road;
                }
            }

            return map;
        }
        private string[] DeleteRoundObstacle(string[] data)
        {
            data[0] = "";
            data[data.Length-1] = "";
            
            for(int i=0;i<data.Length;i++)
            {
                if (data[i].Length>1)
                {
                    data[i] = data[i].Remove(data[i].Length-1,1);
                    data[i] = data[i].Remove(0,1);
                }
                else
                {
                    data[i] = "";
                }
            }
            string[] returnData=OrganizeString(data);

            return returnData;
        }

        private string[] OrganizeString(string[] text)
        {
            string wipData = "";
            string[] returnData;
            foreach (var s in text)
                wipData += s + "\n";
            returnData = wipData.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            return returnData;
        }


        private string[] CheckRoundObstacle(string[] data)
        {
            string upRoundData = data[0];

            string downRoundData = data[data.Length-1];

            string leftRoundData = "";
            foreach (var s in data)
                leftRoundData += s[0];
            string rightRoundData = "";
            foreach (var s in data)
                rightRoundData += s[s.Length-1];

            if (CheckIsAllObstacle(upRoundData) && CheckIsAllObstacle(downRoundData) && 
                CheckIsAllObstacle(leftRoundData) &&CheckIsAllObstacle(rightRoundData))
            {
                data = DeleteRoundObstacle(data);
                CheckRoundObstacle(data);
            }
             
            data= OrganizeString(data);

            return data;
        }

        private bool CheckIsAllObstacle(string text)
        {
            if (CountText(text, "*") == text.Length)
                return true;
            return false;
        }
        private int CountText(string target, string search)
        {
            int cnt = 0;
            bool check = true;

            while (check)
            {
                if (target.IndexOf(search, System.StringComparison.CurrentCulture) == -1)
                {
                    check = false;
                }
                else
                {
                    target = target.Remove(0, target.IndexOf(search, System.StringComparison.CurrentCulture) + 1);
                    cnt++;
                }
            }

            return cnt;
        }
    }
}