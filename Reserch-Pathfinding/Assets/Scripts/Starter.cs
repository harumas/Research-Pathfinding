using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Rendering;
using Visualizer.MapEditor;

public readonly struct TriangleData
{
    public readonly int Id;
    public readonly Vector2 V0;
    public readonly Vector2 V1;
    public readonly Vector2 V2;
    public readonly Vector2 Centroid;

    public TriangleData(int id, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 centroid)
    {
        Id = id;
        V0 = v0;
        V1 = v1;
        V2 = v2;
        Centroid = centroid;
    }
}

/// <summary>
/// メッシュの生成結果
/// </summary>
public class GenerateContext
{
    // 生成されたオブジェクト
    public readonly GameObject GeneratedObject;

    // 三角形の情報
    public readonly Dictionary<int, TriangleData> Triangles;

    // 三角形メッシュ
    public readonly IMesh TMesh;

    // グリッドマップデータ
    public readonly MapData MapData;

    public GenerateContext(GameObject generatedObject, Dictionary<int, TriangleData> triangles, IMesh tMesh, List<Vector2> centroids,
        MapData mapData)
    {
        GeneratedObject = generatedObject;
        Triangles = triangles;
        TMesh = tMesh;
        MapData = mapData;
    }
}

public class Starter : MonoBehaviour
{
    [SerializeField] private MapDataManager mapDataManager;
    [SerializeField] private Material material;
    [SerializeField] private Vector2 displaySize;

    public event Action<GenerateContext> OnMeshGenerated;

    private GridTriangulator triangulator;
    private List<Vector2> trianglePoints = new List<Vector2>();

    private void Start()
    {
        MapData mapData = mapDataManager.Load();
        triangulator = new GridTriangulator(mapData);

        // メッシュの作成
        (GameObject gameObj, IMesh tMesh) polygonObject = CreatePolygonObject();

        // マテリアルの設定
        var meshRenderer = polygonObject.gameObj.GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        // 表示サイズの設定
        var displayScaler = polygonObject.gameObj.AddComponent<DisplayScaler>();
        displayScaler.Scale(displaySize, new Vector2(mapData.Width, mapData.Height));

        var points = CreateTrianglePoints(polygonObject.tMesh);
        var centroids = points.Select(triangle => (triangle.v0 + triangle.v1 + triangle.v2) / 3).ToList();

        var triangles = new Dictionary<int, TriangleData>(points.Count);

        int index = 0;
        foreach (Triangle triangle in polygonObject.tMesh.Triangles)
        {
            var (v0, v1, v2) = points[index];
            var centroid = centroids[index];
            triangles.Add(triangle.ID, new TriangleData(triangle.ID, v0, v1, v2, centroid));
            index++;
        }

        GenerateContext context = new GenerateContext(polygonObject.gameObj, triangles, polygonObject.tMesh, centroids, mapData);
        OnMeshGenerated?.Invoke(context);

        Transform scaler = displayScaler.transform;
        trianglePoints = centroids.Select(triangle => triangle * (Vector2)scaler.localScale + (Vector2)scaler.localPosition).ToList();
    }

    private List<(Vector2 v0, Vector2 v1, Vector2 v2)> CreateTrianglePoints(IMesh tMesh)
    {
        var points = new List<(Vector2 v0, Vector2 v1, Vector2 v2)>();

        foreach (Triangle triangle in tMesh.Triangles)
        {
            var v0 = triangle.GetVertex(0);
            var v1 = triangle.GetVertex(1);
            var v2 = triangle.GetVertex(2);

            var p0 = new Vector3((float)v0.X, (float)v0.Y);
            var p1 = new Vector3((float)v1.X, (float)v1.Y);
            var p2 = new Vector3((float)v2.X, (float)v2.Y);

            points.Add((p0, p1, p2));
        }

        return points;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 trianglePoint in trianglePoints)
        {
            Gizmos.DrawSphere(trianglePoint, 0.02f);
        }
    }

    private (GameObject gameObj, IMesh tMesh) CreatePolygonObject()
    {
        GameObject triangleMesh = new GameObject("TriangleMesh");

        Mesh mesh = new Mesh();

        //メッシュを三角ポリゴンに分割する
        IMesh tMesh = triangulator.Triangulate(mesh);

        MeshFilter mf = triangleMesh.AddComponent<MeshFilter>();
        MeshRenderer renderer = triangleMesh.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        mf.mesh = mesh;

        return (triangleMesh, tMesh);
    }
}