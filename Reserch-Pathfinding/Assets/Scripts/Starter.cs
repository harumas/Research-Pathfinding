using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Visualizer.MapEditor;

namespace DefaultNamespace
{
    public readonly struct GridVertex : IEquatable<GridVertex>
    {
        public readonly int[] Vertices;

        public GridVertex(int a, int b, int c, int d)
        {
            Vertices = new int[] { a, b, c, d };
        }

        public bool Equals(GridVertex other)
        {
            return Vertices[0] == other.Vertices[0] &&
                   Vertices[1] == other.Vertices[1] &&
                   Vertices[2] == other.Vertices[2] &&
                   Vertices[3] == other.Vertices[3];
        }

        public override int GetHashCode()
        {
            return (Vertices != null ? Vertices.GetHashCode() : 0);
        }
    }

    public class Starter : MonoBehaviour
    {
        [SerializeField] private MapDataManager mapDataManager;

        private Dictionary<GridVertex, GridType> vertexGridDictionary;

        private void Start()
        {
            MapData mapData = mapDataManager.Load();
            Vector2[] vertices = new Vector2[(mapData.Width + 1) * (mapData.Height + 1)];
            vertexGridDictionary = new Dictionary<GridVertex, GridType>();

            for (int y = 0; y < mapData.Height + 1; y++)
            {
                for (int x = 0; x < mapData.Width + 1; x++)
                {
                    vertices[y * (mapData.Width + 1) + x] = new Vector2(x, y);
                    Debug.Log($"{y * mapData.Width + x}: {new Vector2(x, y)}");
                }
            }


            for (int y = 0; y < mapData.Height; y++)
            {
                for (int x = 0; x < mapData.Width; x++)
                {
                    int a = y * (mapData.Width + 1) + x;
                    int b = a + 1;
                    int c = a + y * (mapData.Width + 1);
                    int d = c + 1;

                    vertexGridDictionary.Add(new GridVertex(a, b, c, d), mapData.Grids[y, x]);
                }
            }

            var triangulator = new Triangulator();
            GameObject polygonObject = triangulator.CreateInfluencePolygon(vertices, vertexGridDictionary);

            var objMesh = polygonObject.GetComponent<MeshFilter>();
            List<int> triangles = objMesh.mesh.triangles.ToList();


            Debug.Log(triangles.Count);
        }
    }
}