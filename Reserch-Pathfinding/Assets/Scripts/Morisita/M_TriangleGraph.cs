using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using Unity.VisualScripting;
using UnityEngine;
using Visualizer.MapEditor;

public class M_TriangleGraph
{
    private readonly Dictionary<int, List<TriangleData>> triangleGraph = new Dictionary<int, List<TriangleData>>();
    private readonly Dictionary<int, TriangleData> triangles;
    private readonly M_GenerateContext context;

    public M_TriangleGraph(M_GenerateContext context)
    {
        this.context = context;

        //Debug.Log("Triangles.Count:" + context.Triangles.Count);
        //int count = 0;
        //foreach (var t in context.Triangles)
        //{
        //    count++;
        //    Debug.Log("t" + count + ":" + t);
        //}

        foreach (Triangle triangle in context.TMesh.Triangles)
        {
            List<TriangleData> data = new List<ITriangle>()
                {
                    triangle.GetNeighbor(0),
                    triangle.GetNeighbor(1),
                    triangle.GetNeighbor(2)
                }
                .Where(item => item != null)
                .Select(neighbor => context.Triangles[neighbor.ID])
                .Where(item => !IsObstacle(item))
                .ToList();

            triangleGraph.Add(triangle.ID, data);
        }

        triangles = context.Triangles;

        Debug.Log("triangles.count" + triangles.Count);
        foreach(var t in triangles)
        {
            Debug.Log("id : "+t.Value.Id);
        }
    }

    private bool IsObstacle(TriangleData triangleData)
    {
        Vector2Int gridPoint = GetGridPoint(triangleData);
        var test = context.MapData.Grids[gridPoint.y, gridPoint.x];
        return test == GridType.Obstacle;
    }


    private Vector2Int GetGridPoint(TriangleData triangleData)
    {
        MapData mapData = context.MapData;
        Vector2 centroid = triangleData.Centroid;
        //Debug.Log("before:" + centroid.x);
        var xy= new Vector2Int((int)(Mathf.Round(centroid.x + 0.5f) - 1f), (int)(Mathf.RoundToInt(centroid.y + 0.5f) - 1f));

        return xy;
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