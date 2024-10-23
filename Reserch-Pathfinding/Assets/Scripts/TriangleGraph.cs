using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using UnityEngine;

public class TriangleGraph
{
    private readonly Dictionary<int, List<TriangleData>> triangleGraph = new Dictionary<int, List<TriangleData>>();
    private readonly Dictionary<int, TriangleData> triangles;

    public TriangleGraph(GenerateContext context)
    {
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
                .ToList();


            triangleGraph.Add(triangle.ID, data);
        }

        triangles = context.Triangles;
    }

    public List<TriangleData> GetNeighbors(int id)
    {
        return triangleGraph[id];
    }

    public TriangleData GetTriangle(int id)
    {
        return triangles[id];
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