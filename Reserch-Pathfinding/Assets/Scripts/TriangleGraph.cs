using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using UnityEngine;
using Visualizer.MapEditor;

public class TriangleGraph
{
    private readonly Dictionary<int, List<TriangleData>> triangleGraph = new Dictionary<int, List<TriangleData>>();
    private readonly Dictionary<int, TriangleData> triangles;
    private readonly GenerateContext context;

    public TriangleGraph(GenerateContext context)
    {
        this.context = context;
        
        foreach (Triangle triangle in context.TMesh.Triangles)
        {
            // if (IsObstacle(context.Triangles[triangle.ID]))
            // {
            //     continue;
            // }

            List<TriangleData> data = new List<ITriangle>()
                {
                    triangle.GetNeighbor(0),
                    triangle.GetNeighbor(1),
                    triangle.GetNeighbor(2)
                }
                .Where(item => item != null)
                .Select(neighbor => context.Triangles[neighbor.ID])
                //.Where(item => IsObstacle(item))
                .ToList();

            triangleGraph.Add(triangle.ID, data);
        }

        triangles = context.Triangles;
    }

    private bool IsObstacle(TriangleData triangleData)
    {
        Vector2Int gridPoint = GetGridPoint(triangleData);
        return context.MapData.Grids[gridPoint.y, gridPoint.x] == GridType.Obstacle;
    }

    private Vector2Int GetGridPoint(TriangleData triangleData)
    {
        MapData mapData = context.MapData;
        Vector2 centroid = triangleData.Centroid;
        centroid += new Vector2(mapData.Width * 0.5f, mapData.Height * 0.5f);

        return new Vector2Int(Mathf.RoundToInt(centroid.x), Mathf.RoundToInt(centroid.y));
    }

    public Dictionary<int, TriangleData> GetTriangles()
    {
        return triangles;
    }

    public ICollection<KeyValuePair<int, List<TriangleData>>> GetGraphCollection()
    {
        return triangleGraph;
    }
}