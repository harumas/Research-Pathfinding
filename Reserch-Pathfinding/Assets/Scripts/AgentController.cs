using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;

namespace DefaultNamespace
{
    public class AgentController : MonoBehaviour
    {
        [SerializeField] private GameObject agentPrefab;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Starter starter;

        private Transform scaler;
        private TriangleGraph triangleGraph;
        private HalfwaySolver halfwaySolver;
        private Transform agent;
        private Transform enemy;
        private List<Node> currentPath;

        private List<Vector2> triangleCentroids;
        private List<Vector2> gridPoints;

        private void Awake()
        {
            starter.OnMeshGenerated += OnMeshGenerated;
        }

        private void OnMeshGenerated(GenerateContext context)
        {
            triangleCentroids = context.Triangles.Values.Select(item => item.Centroid).ToList();

            gridPoints = new List<Vector2>();
            for (int y = 0; y < context.MapData.Height; y++)
            {
                for (int x = 0; x < context.MapData.Width; x++)
                {
                    gridPoints.Add(new Vector2(x, y));
                }
            }

            scaler = context.GeneratedObject.GetComponent<DisplayScaler>().transform;

            triangleGraph = new TriangleGraph(context);
            (int agentIndex, int enemyIndex) = CreateAgents(context.Triangles);

            halfwaySolver = new HalfwaySolver(triangleGraph);
            List<Node> nodes = halfwaySolver.Solve(enemyIndex, agentIndex);
            currentPath = nodes;
        }

        private void OnDrawGizmos()
        {
            if (currentPath == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Vector2 a = (Vector2)currentPath[i].Position * scaler.localScale + (Vector2)scaler.localPosition;
                Vector2 b = (Vector2)currentPath[i + 1].Position * scaler.localScale + (Vector2)scaler.localPosition;
                Gizmos.DrawLine(a, b);
            }

           // if (triangleCentroids != null && gridPoints != null)
           //   {
           //       foreach (Vector2 centroid in triangleCentroids)
           //       {
           //           Gizmos.color = Color.blue;
           //           Gizmos.DrawSphere(centroid, 0.1f);
           //       }
           //
           //       foreach (Vector2 gridPoint in gridPoints)
           //       {
           //           Gizmos.color = Color.green;
           //           Gizmos.DrawSphere(gridPoint, 0.1f);
           //       }
           //   } 
        }

        private (int agentIndex, int enemyIndex) CreateAgents(Dictionary<int, TriangleData> triangles)
        {
            int agentIndex = Random.Range(0, triangles.Count);
            var agentTriangle = triangles.ElementAt(agentIndex);
            Vector3 agentPos = agentTriangle.Value.Centroid * scaler.localScale + (Vector2)scaler.localPosition;

            agent = Instantiate(agentPrefab, agentPos, Quaternion.identity).transform;

            var enemyTriangle = triangles.OrderByDescending(item => Vector3.Distance(item.Value.Centroid, agentPos)).First();
            Vector3 enemyPos = enemyTriangle.Value.Centroid * scaler.localScale + (Vector2)scaler.localPosition;
            ;

            enemy = Instantiate(enemyPrefab, enemyPos, Quaternion.identity).transform;

            return (agentTriangle.Key, enemyTriangle.Key);
        }
    }
}