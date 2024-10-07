using System;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Rendering;
using Visualizer.MapEditor;

public class Starter : MonoBehaviour
{
    [SerializeField] private MapDataManager mapDataManager;
    [SerializeField] private Material material;
    [SerializeField] private Vector2 displaySize;

    private GridTriangulator triangulator;

    private void Awake()
    {
        MapData mapData = mapDataManager.Load();
        triangulator = new GridTriangulator(mapData);

        // メッシュの作成
        GameObject polygonObject = CreatePolygonObject();

        // マテリアルの設定
        var meshRenderer = polygonObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        // 表示サイズの設定
        var displayScaler = polygonObject.AddComponent<DisplayScaler>();
        displayScaler.Scale(displaySize, new Vector2(mapData.Width, mapData.Height));
    }

    private GameObject CreatePolygonObject()
    {
        GameObject triangleMesh = new GameObject("TriangleMesh");

        Mesh mesh = new Mesh();
        
        //メッシュを三角ポリゴンに分割する
        triangulator.Triangulate(mesh);
        
        MeshFilter mf = triangleMesh.AddComponent<MeshFilter>();
        MeshRenderer renderer = triangleMesh.AddComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        mf.mesh = mesh;

        return triangleMesh;
    }
}