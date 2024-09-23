using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

struct Triangle
{
    public int P1;
    public int P2;
    public int P3;

    public Triangle(int point1, int point2, int point3)
    {
        P1 = point1;
        P2 = point2;
        P3 = point3;
    }
}

class Edge
{
    public int P1;
    public int P2;

    public Edge(int point1, int point2)
    {
        P1 = point1;
        P2 = point2;
    }

    public Edge() : this(0, 0)
    {
    }

    public bool Equals(Edge other)
    {
        return ((this.P1 == other.P2) && (this.P2 == other.P1)) || ((this.P1 == other.P1) && (this.P2 == other.P2));
    }
}


public class Triangulator
{
    bool TriangulatePolygonSubFunc_InCircle(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        if (Mathf.Abs(p1.y - p2.y) < float.Epsilon && Mathf.Abs(p2.y - p3.y) < float.Epsilon)
        {
            return false;
        }

        float m1, m2, mx1, mx2, my1, my2, xc, yc;
        if (Mathf.Abs(p2.y - p1.y) < float.Epsilon)
        {
            m2 = -(p3.x - p2.x) / (p3.y - p2.y);
            mx2 = (p2.x + p3.x) * 0.5f;
            my2 = (p2.y + p3.y) * 0.5f;
            xc = (p2.x + p1.x) * 0.5f;
            yc = m2 * (xc - mx2) + my2;
        }
        else if (Mathf.Abs(p3.y - p2.y) < float.Epsilon)
        {
            m1 = -(p2.x - p1.x) / (p2.y - p1.y);
            mx1 = (p1.x + p2.x) * 0.5f;
            my1 = (p1.y + p2.y) * 0.5f;
            xc = (p3.x + p2.x) * 0.5f;
            yc = m1 * (xc - mx1) + my1;
        }
        else
        {
            m1 = -(p2.x - p1.x) / (p2.y - p1.y);
            m2 = -(p3.x - p2.x) / (p3.y - p2.y);
            mx1 = (p1.x + p2.x) * 0.5f;
            mx2 = (p2.x + p3.x) * 0.5f;
            my1 = (p1.y + p2.y) * 0.5f;
            my2 = (p2.y + p3.y) * 0.5f;
            xc = (m1 * mx1 - m2 * mx2 + my2 - my1) / (m1 - m2);
            yc = m1 * (xc - mx1) + my1;
        }

        float dx = p2.x - xc;
        float dy = p2.y - yc;
        float rsqr = dx * dx + dy * dy;
        dx = p.x - xc;
        dy = p.y - yc;
        double drsqr = dx * dx + dy * dy;
        return (drsqr <= rsqr);
    }


    public GameObject CreateInfluencePolygon(Vector2[] xZofVertices)
    {
        Vector3[] vertices = new Vector3[xZofVertices.Length];
        for (int ii1 = 0; ii1 < xZofVertices.Length; ii1++)
        {
            vertices[ii1] = new Vector3(xZofVertices[ii1].x, 0, xZofVertices[ii1].y);
        }

        GameObject ourNewMesh = new GameObject("OurNewMesh1");
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = xZofVertices,
            triangles = TriangulatePolygon(xZofVertices)
        };
        mesh.RecalculateNormals();
        MeshFilter mf = ourNewMesh.AddComponent<MeshFilter>();
        MeshRenderer renderer = ourNewMesh.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        mf.mesh = mesh;

        ourNewMesh.transform.Rotate(new Vector3(-90f, 0f, 0f));

        return ourNewMesh;
    }


    private int[] TriangulatePolygon(Vector2[] xZofVertices)
    {
        int vertexCount = xZofVertices.Length;
        float xmin = xZofVertices[0].x;
        float ymin = xZofVertices[0].y;
        float xmax = xmin;
        float ymax = ymin;
        for (int ii1 = 1; ii1 < vertexCount; ii1++)
        {
            if (xZofVertices[ii1].x < xmin)
            {
                xmin = xZofVertices[ii1].x;
            }
            else if (xZofVertices[ii1].x > xmax)
            {
                xmax = xZofVertices[ii1].x;
            }

            if (xZofVertices[ii1].y < ymin)
            {
                ymin = xZofVertices[ii1].y;
            }
            else if (xZofVertices[ii1].y > ymax)
            {
                ymax = xZofVertices[ii1].y;
            }
        }

        float dx = xmax - xmin;
        float dy = ymax - ymin;
        float dmax = (dx > dy) ? dx : dy;
        float xmid = (xmax + xmin) * 0.5f;
        float ymid = (ymax + ymin) * 0.5f;
        Vector2[] expandedXZ = new Vector2[3 + vertexCount];
        for (int ii1 = 0; ii1 < vertexCount; ii1++)
        {
            expandedXZ[ii1] = xZofVertices[ii1];
        }

        expandedXZ[vertexCount] = new Vector2((xmid - 2 * dmax), (ymid - dmax));
        expandedXZ[vertexCount + 1] = new Vector2(xmid, (ymid + 2 * dmax));
        expandedXZ[vertexCount + 2] = new Vector2((xmid + 2 * dmax), (ymid - dmax));
        List<Triangle> triangleList = new List<Triangle>();
        triangleList.Add(new Triangle(vertexCount, vertexCount + 1, vertexCount + 2));
        for (int ii1 = 0; ii1 < vertexCount; ii1++)
        {
            List<Edge> edges = new List<Edge>();
            for (int ii2 = 0; ii2 < triangleList.Count; ii2++)
            {
                if (TriangulatePolygonSubFunc_InCircle(expandedXZ[ii1], expandedXZ[triangleList[ii2].P1], expandedXZ[triangleList[ii2].P2],
                        expandedXZ[triangleList[ii2].P3]))
                {
                    edges.Add(new Edge(triangleList[ii2].P1, triangleList[ii2].P2));
                    edges.Add(new Edge(triangleList[ii2].P2, triangleList[ii2].P3));
                    edges.Add(new Edge(triangleList[ii2].P3, triangleList[ii2].P1));
                    triangleList.RemoveAt(ii2);
                    ii2--;
                }
            }

            if (ii1 >= vertexCount)
            {
                continue;
            }

            for (int ii2 = edges.Count - 2; ii2 >= 0; ii2--)
            {
                for (int ii3 = edges.Count - 1; ii3 >= ii2 + 1; ii3--)
                {
                    if (edges[ii2].Equals(edges[ii3]))
                    {
                        edges.RemoveAt(ii3);
                        edges.RemoveAt(ii2);
                        ii3--;
                        continue;
                    }
                }
            }

            for (int ii2 = 0; ii2 < edges.Count; ii2++)
            {
                triangleList.Add(new Triangle(edges[ii2].P1, edges[ii2].P2, ii1));
            }

            edges.Clear();
            edges = null;
        }

        for (int ii1 = triangleList.Count - 1; ii1 >= 0; ii1--)
        {
            if (triangleList[ii1].P1 >= vertexCount || triangleList[ii1].P2 >= vertexCount || triangleList[ii1].P3 >= vertexCount)
            {
                triangleList.RemoveAt(ii1);
            }
        }

        triangleList.TrimExcess();
        int[] triangles = new int[3 * triangleList.Count];
        for (int ii1 = 0; ii1 < triangleList.Count; ii1++)
        {
            triangles[3 * ii1] = triangleList[ii1].P1;
            triangles[3 * ii1 + 1] = triangleList[ii1].P2;
            triangles[3 * ii1 + 2] = triangleList[ii1].P3;
        }

        return triangles;
    }
}