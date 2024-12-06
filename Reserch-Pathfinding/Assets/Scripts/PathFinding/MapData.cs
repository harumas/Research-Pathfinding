using System;
using System.Collections.Generic;
using System.Linq;
using PathFinder.Core;

namespace PathFinding
{
    [Flags]
    public enum GridType
    {
        Road = 1,
        Obstacle = 2,
        Path = 4,
        CorrectCircle = 8,
        IncorrectCircle = 16,
        DebugPath = 32,
    }

    public class MapData
    {
        public readonly int Height;
        public readonly int Width;
        public readonly int PassableCount;
        public readonly Vector2Int Player;
        public readonly Vector2Int Enemy;
        public readonly GridType[,] Grids;

        public MapData(
            int height,
            int width,
            int passableCount,
            Vector2Int start,
            Vector2Int goal,
            GridType[,] grids
        )
        {
            Height = height;
            Width = width;
            Player = start;
            Enemy = goal;
            PassableCount = passableCount;
            Grids = grids;
        }
    }


}