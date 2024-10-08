using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using UnityEngine;
using Visualizer.MapEditor;
using Wanna.DebugEx;

[Flags]
public enum Corner : byte
{
    None = 0b0000,
    LeftBottom = 0b0001,
    LeftTop = 0b0010,
    RightTop = 0b0100,
    RightBottom = 0b1000,
    Left = LeftBottom | LeftTop,
    Right = RightTop | RightBottom,
    Top = LeftTop | RightTop,
    Bottom = LeftBottom | RightBottom,
}

public readonly struct Edge : IEquatable<Edge>
{
    public readonly Vector2Int From;
    public readonly Vector2Int To;

    public Edge(Vector2Int from, Vector2Int to)
    {
        From = from;
        To = to;
    }

    public Edge Swap()
    {
        return new Edge(To, From);
    }

    public bool Equals(Edge other)
    {
        return From.Equals(other.From) && To.Equals(other.To) || From.Equals(other.To) && To.Equals(other.From);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(From, To);
    }
}

public class GridTriangulator
{
    private readonly MapData mapData;
    private readonly GridType[,] gridData;

    public GridTriangulator(MapData mapData)
    {
        this.mapData = mapData;
        gridData = new GridType[mapData.Height, mapData.Width];
        Array.Copy(mapData.Grids, gridData, mapData.Grids.Length);

        // 配列の行数と列数を取得
        int rows = gridData.GetLength(0);
        int cols = gridData.GetLength(1);

        // 各列を反転
        for (int j = 0; j < cols; j++)
        {
            // 上の行と下の行を入れ替える処理
            for (int i = 0; i < rows / 2; i++)
            {
                // 上の行の要素と下の行の要素を交換
                GridType temp = gridData[i, j];
                gridData[i, j] = gridData[rows - 1 - i, j];
                gridData[rows - 1 - i, j] = temp;
            }
        }
    }

    public void Triangulate(Mesh mesh)
    {
        // ポリゴンの頂点を作成
        List<Vertex> vertices = CreateVertices();
        List<Segment> segments = CreateSegments(vertices);

        // ポリゴンのインスタンスを作成
        var polygon = new Polygon(vertices.Count);

        // 頂点と制約のセグメントを追加
        vertices.ForEach(vertex => polygon.Add(vertex));
        segments.ForEach(segment => polygon.Add(segment));

        // 三角化
        IMesh polygonMesh = polygon.Triangulate(new ConstraintOptions() { ConformingDelaunay = true });

        // メッシュ用に変換
        Vector3[] verticesList = vertices.Select(vertex => new Vector3((float)vertex.X, (int)vertex.Y, 0f)).ToArray();
        Vector2[] uvList = vertices.Select(vertex => new Vector2((float)vertex.X, (float)vertex.Y)).ToArray();
        List<int> triangles = GetTriangles(polygonMesh, vertices.Count);

        mesh.vertices = verticesList.ToArray();
        mesh.uv = uvList.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    private List<int> GetTriangles(IMesh polygonMesh, int verticesCount)
    {
        List<int> triangles = new List<int>(verticesCount * 3);
        var tt = polygonMesh.Triangles.ToList();
        foreach (Triangle triangle in polygonMesh.Triangles)
        {
            triangles.Add(triangle.GetVertexID(2));
            triangles.Add(triangle.GetVertexID(1));
            triangles.Add(triangle.GetVertexID(0));
        }

        return triangles;
    }

    private List<Vertex> CreateVertices()
    {
        var vertices = new List<Vertex>();

        for (int y = 0; y < mapData.Height + 1; y++)
        {
            for (int x = 0; x < mapData.Width + 1; x++)
            {
                bool isLeftBottom = y == 0 && x == 0;
                bool isLeftTop = y == mapData.Height && x == 0;
                bool isRightBottom = y == 0 && x == mapData.Width;
                bool isRightTop = y == mapData.Height && x == mapData.Width;

                // 四つ角は頂点を作成
                if (isLeftBottom || isLeftTop || isRightBottom || isRightTop)
                {
                    vertices.Add(new Vertex(x, y));
                    continue;
                }

                Corner corners = GetCorners(x, y);

                // 障害物に触れてない場合 or 角じゃない場合は頂点を作成しない
                if (corners == Corner.None || corners == Corner.Left || corners == Corner.Right || corners == Corner.Top || corners == Corner.Bottom)
                {
                    continue;
                }

                vertices.Add(new Vertex(x, y));
            }
        }

        return vertices;
    }


    private List<Segment> CreateSegments(List<Vertex> vertices)
    {
        // 頂点座標と頂点のインスタンスのマップを作成
        var vertexMap = vertices.ToDictionary(vertex => new Vector2Int((int)vertex.X, (int)vertex.Y), vertex => vertex);

        HashSet<Edge> edges = GetOutlineSegments(vertexMap);

        // 障害物のグリッドを取得
        var obstacleGrids = GetObstacleGrids();

        DebugEx.Log(obstacleGrids);

        // 外周のグリッドを取得
        var outlineGrids = GetOutlineGrids(obstacleGrids);

        foreach (List<Vector2Int> outlineGrid in outlineGrids)
        {
            DebugEx.Log(outlineGrid);
        }

        // 外周のグリッド群からセグメントを作成
        foreach (List<Vector2Int> outlines in outlineGrids)
        {
            //外周のグリッド群に属する頂点を取得
            List<Vector2Int> belongVertices = GetBelongVertices(outlines, vertexMap.Keys);

            //外周のグリッド群を囲むセグメントを取得して登録
            List<Edge> belongEdges = FindSegments(belongVertices, vertexMap);
            foreach (Edge edge in belongEdges)
            {
                if (edges.Contains(edge))
                {
                    continue;
                }

                edges.Add(edge);
            }
        }

        return edges.Select(edge => new Segment(vertexMap[edge.From], vertexMap[edge.To])).ToList();
    }

    private HashSet<Edge> GetOutlineSegments(Dictionary<Vector2Int, Vertex> gridVertexMap)
    {
        var edges = new List<Edge>();

        Vector2Int from = new Vector2Int(0, 0);
        for (int x = 1; x < mapData.Width + 1; x++)
        {
            Vector2Int pos = new Vector2Int(x, 0);

            if (gridVertexMap.ContainsKey(pos))
            {
                edges.Add(new Edge(from, pos));
                from = pos;
            }
        }

        from = new Vector2Int(mapData.Width, 0);
        for (int y = 1; y < mapData.Height + 1; y++)
        {
            Vector2Int pos = new Vector2Int(mapData.Width, y);

            if (gridVertexMap.ContainsKey(pos))
            {
                edges.Add(new Edge(from, pos));
                from = pos;
            }
        }

        from = new Vector2Int(mapData.Width, mapData.Height);
        for (int x = mapData.Width - 1; x >= 0; x--)
        {
            Vector2Int pos = new Vector2Int(x, mapData.Height);

            if (gridVertexMap.ContainsKey(pos))
            {
                edges.Add(new Edge(from, pos));
                from = pos;
            }
        }

        from = new Vector2Int(0, mapData.Height);
        for (int y = mapData.Height - 1; y >= 0; y--)
        {
            Vector2Int pos = new Vector2Int(0, y);

            if (gridVertexMap.ContainsKey(pos))
            {
                edges.Add(new Edge(from, pos));
                from = pos;
            }
        }

        return new HashSet<Edge>(edges);
    }

    private List<Edge> FindSegments(List<Vector2Int> belongVertices, Dictionary<Vector2Int, Vertex> vertexMap)
    {
        var edges = new HashSet<Edge>();

        var queue = new Queue<Edge>();
        var closedSet = new List<Edge>();

        // 最初の頂点座標の登録
        Edge first = new Edge(belongVertices[0], belongVertices[0]);
        queue.Enqueue(first);
        closedSet.Add(first);

        var addDirectionQueue = new Queue<Vector2Int>();

        // 幅優先探索でセグメントを取得
        while (queue.Count > 0)
        {
            Edge edge = queue.Dequeue();
            Vector2Int from = edge.From;

            if (edge.From != edge.To && belongVertices.Contains(edge.To))
            {
                // 探索している座標が存在する頂点の場合はセグメントを作成
                edges.Add(edge);
                from = edge.To;
            }

            // 次に探索する頂点座標を取得
            FindNextVertexPos(edge.To, addDirectionQueue);

            foreach (Vector2Int dir in addDirectionQueue)
            {
                Edge next = new Edge(from, edge.To + dir);

                // すでに探索済みの場合はスキップ
                if (closedSet.Contains(next))
                {
                    continue;
                }

                queue.Enqueue(next);
                closedSet.Add(next);
            }

            addDirectionQueue.Clear();
        }

        return edges.ToList();
    }

    private void FindNextVertexPos(Vector2Int point, Queue<Vector2Int> addDirectionQueue)
    {
        // 現在の座標の周囲の障害物の情報を取得
        Corner corners = GetCorners(point.x, point.y);

        // 左側に辺を作れる場合は左方向に探索
        Corner leftSide = corners & Corner.Left;
        if (leftSide == Corner.LeftTop || leftSide == Corner.LeftBottom)
        {
            addDirectionQueue.Enqueue(new Vector2Int(-1, 0));
        }

        // 上側
        Corner upSide = corners & Corner.Top;
        if (upSide == Corner.LeftTop || upSide == Corner.RightTop)
        {
            addDirectionQueue.Enqueue(new Vector2Int(0, 1));
        }

        // 右側
        Corner rightSide = corners & Corner.Right;
        if (rightSide == Corner.RightTop || rightSide == Corner.RightBottom)
        {
            addDirectionQueue.Enqueue(new Vector2Int(1, 0));
        }

        // 下側
        Corner bottomSide = corners & Corner.Bottom;
        if (bottomSide == Corner.LeftBottom || bottomSide == Corner.RightBottom)
        {
            addDirectionQueue.Enqueue(new Vector2Int(0, -1));
        }
    }

    private List<Vector2Int> GetBelongVertices(List<Vector2Int> grids, ICollection<Vector2Int> vertices)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>(vertices);
        HashSet<Vector2Int> belongVertices = new HashSet<Vector2Int>();

        var directions = new Vector2Int[]
        {
            new Vector2Int(0, 0), //左下
            new Vector2Int(0, 1), //左上
            new Vector2Int(1, 1), //右上
            new Vector2Int(1, 0) //右下
        };

        foreach (Vector2Int pos in grids)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = pos + dir;

                if (points.Contains(next))
                {
                    points.Remove(next);
                    belongVertices.Add(next);
                }
            }
        }

        return belongVertices.ToList();
    }

    private List<List<Vector2Int>> GetOutlineGrids(List<Vector2Int> obstacles)
    {
        var obstacleSet = new HashSet<Vector2Int>(obstacles);

        var directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(1, 1),
        };

        //幅優先探索で外周の座標を取得
        var outlines = new List<List<Vector2Int>>();
        while (obstacleSet.Count > 0)
        {
            Vector2Int start = obstacleSet.First();
            obstacleSet.Remove(start);

            var outline = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                outline.Add(current);

                foreach (Vector2Int direction in directions)
                {
                    Vector2Int next = current + direction;

                    // 障害物のグリッドでない場合はスキップ
                    if (!obstacleSet.Contains(next))
                    {
                        continue;
                    }

                    //外側に位置するグリッドかどうか
                    bool isOutline = directions.Select(dir => next + dir)
                        .All(pos =>
                            ((pos.x < 0 || pos.x >= mapData.Width) ||
                             (pos.y < 0 || pos.y >= mapData.Height) ||
                             ((mapData.Grids[pos.y, pos.x] & GridType.Road)) == 0));

                    // 外周のグリッドの場合は外周に追加
                    if (isOutline)
                    {
                        outline.Add(next);
                    }

                    obstacleSet.Remove(next);
                    queue.Enqueue(next);
                }
            }

            outlines.Add(outline);
        }

        return outlines;
    }

    private List<Vector2Int> GetObstacleGrids()
    {
        List<Vector2Int> obstacleGrids = new List<Vector2Int>(gridData.Length - mapData.PassableCount);

        // 障害物のグリッドを取得
        for (int y = 0; y < mapData.Height; y++)
        {
            for (int x = 0; x < mapData.Width; x++)
            {
                if ((gridData[y, x] & GridType.Obstacle) != 0)
                {
                    obstacleGrids.Add(new Vector2Int(x, y));
                }
            }
        }

        return obstacleGrids;
    }

    private Corner GetCorners(int x, int y)
    {
        Corner corners = 0;

        // 左下
        if (y > 0 && x > 0)
        {
            if ((gridData[y - 1, x - 1] & GridType.Obstacle) != 0)
            {
                corners |= Corner.LeftBottom;
            }
        }

        // 左上
        if (y < mapData.Height && x > 0)
        {
            if ((gridData[y, x - 1] & GridType.Obstacle) != 0)
            {
                corners |= Corner.LeftTop;
            }
        }

        // 右上
        if (y < mapData.Height && x < mapData.Width)
        {
            if ((gridData[y, x] & GridType.Obstacle) != 0)
            {
                corners |= Corner.RightTop;
            }
        }

        // 右下
        if (y > 0 && x < mapData.Width)
        {
            if ((gridData[y - 1, x] & GridType.Obstacle) != 0)
            {
                corners |= Corner.RightBottom;
            }
        }

        return corners;
    }
}