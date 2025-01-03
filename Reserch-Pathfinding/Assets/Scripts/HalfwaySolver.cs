using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using PathFinding.Algorithm;
using UnityEngine;
using Vector2 = Core.Vector2;

public class HalfwaySolver
{
    private readonly ConstrainedAStar pathFinder;
    private readonly Graph graph;
    private readonly Dictionary<int, Node> nodes;

    public HalfwaySolver(M_TriangleGraph triangleGraph)
    {
        graph = CreateGraph(triangleGraph);
        nodes = CreateNodes(triangleGraph);
        pathFinder = new ConstrainedAStar(graph, nodes);
    }
    public HalfwaySolver(TriangleGraph triangleGraph)
    {
        graph = CreateGraph(triangleGraph);
        nodes = CreateNodes(triangleGraph);
        pathFinder = new ConstrainedAStar(graph, nodes);
    }

    public List<Node> Solve(int startId, int goalId)
    {
        //int halfGoal = CalculateHalfGoal(startId, goalId);
        //return pathFinder.FindPath(startId, halfGoal);
        return pathFinder.FindPath(startId, goalId);
    }

    private Dictionary<int, Node> CreateNodes(M_TriangleGraph triangleGraph)
    {
        return triangleGraph
            .GetTriangles()
            .Select(item => new KeyValuePair<int, Node>(item.Key, new Node(item.Key, item.Value.Centroid)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }
    private Dictionary<int, Node> CreateNodes(TriangleGraph triangleGraph)
    {
        return triangleGraph
            .GetTriangles()
            .Select(item => new KeyValuePair<int, Node>(item.Key, new Node(item.Key, item.Value.Centroid)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    private Graph CreateGraph(M_TriangleGraph triangleGraph)
    {
        var triangles = triangleGraph.GetGraphCollection();
        var graph = new Graph(triangles.Count);

        foreach (var (id, neighbors) in triangleGraph.GetGraphCollection())
        {
            foreach (int neighbor in neighbors.Select(triangle => triangle.Id))
            {
                graph.AddEdge(id, neighbor);
            }
        }

        return graph;
    }
    private Graph CreateGraph(TriangleGraph triangleGraph)
    {
        var triangles = triangleGraph.GetGraphCollection();
        var graph = new Graph(triangles.Count);

        foreach (var (id, neighbors) in triangleGraph.GetGraphCollection())
        {
            foreach (int neighbor in neighbors.Select(triangle => triangle.Id))
            {
                graph.AddEdge(id, neighbor);
            }
        }

        return graph;
    }

    private int CalculateHalfGoal(int startId, int goalId)
    {
        const int detail = 8;
        const double delta = Math.PI * 2f / detail;

        var startPos = nodes[startId].Position;
        var goalPos = nodes[goalId].Position;
        var v = goalPos - startPos;

        Node target = null;

        for (int i = 0; i < 8; i++)
        {
            int sign = i % 2 == 0 ? 1 : -1;
            double alpha = delta * (i * sign);
            double sin = Math.Sin(alpha);
            double cos = Math.Cos(alpha);

            Vector2 next = new Vector2((float)(v.x * cos - v.y * sin), (float)(v.x * sin + v.y * cos));
            Vector2 halfPoint = startPos + next * 0.5f;
            target = GetClosestTriangle(halfPoint);

            if (pathFinder.FindPath(nodes[goalId].Index, target.Index) != null)
            {
                break;
            }
        }

        // 見つからなかったらゴールをそのまま返す
        if (target == null)
        {
            return goalId;
        }

        return target.Index;
    }

    private Node GetClosestTriangle(Vector2 point)
    {
        Node closestNode = null;
        float minDistance = float.MaxValue;

        foreach (Node node in nodes.Values)
        {
            Vector2 diff = (point - node.Position);
            float distance = diff.x * diff.x + diff.y * diff.y;

            if (distance < minDistance)
            {
                closestNode = node;
                minDistance = distance;
            }
        }

        return closestNode;
    }
}