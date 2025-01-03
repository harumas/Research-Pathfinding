using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Rendering;
using Visualizer.MapEditor;


//シリアライズされた子要素クラス
[System.Serializable]
public class ChildArray
{
    public List<int> childArray;
}

/// <summary>
/// 障害物の部分のメッシュを消す処理を行うスクリプト
/// </summary>
public class MeshTest : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh myMesh;
    [SerializeField,Header("メッシュを消す")] bool isDelete;
    [SerializeField,Header("ログを出力する")] bool isDebugLog;

    [SerializeField]
    List<int> triangles = new List<int>();

    [SerializeField,Header("重心のリストがなぜかマップと上下が逆なので下の行から入れる")]
    List<ChildArray> Grids;
    [SerializeField]
    List<Vector2> OriginalXY = new List<Vector2>();
    [SerializeField]
    List<Vector2Int> RoundXY = new List<Vector2Int>();

    public void DeleteBlockMesh(M_GenerateContext context)
    {
        // いったん何をもってきてるのかを置いておく
        // 生成された三角形に分割されたメッシュオブジェクト
        GameObject GeneObjext = context.GeneratedObject;

        // 三角形の頂点情報
        List<(Vector2 v0, Vector2 v2, Vector2 v3)> GeneTriangles = context.TrianglesVertices;

        // 三角形の重心
        // これを丸めてグリッド座標にしてマップ情報を参照する
        List<Vector2> GeneCentroids = new(context.Centroids);

        // グリッドマップデータ
        MapData GeneMapData = context.MapData;

        MeshFilter meshf = GeneObjext.GetComponent<MeshFilter>();
        Mesh mesh = meshf.mesh;

        // 三角形の重心を見るために取り出す
        for (int i = 0; i < GeneCentroids.Count(); i++)
        {
            OriginalXY.Add(new Vector2(GeneCentroids[i].x,GeneCentroids[i].y));
        }

        // 三角形の重心を丸める
        for (int i = 0; i < GeneCentroids.Count(); i++)
        {
            // 丸め
            GeneCentroids[i] = new Vector2(MathF.Round(GeneCentroids[i].x+0.5f)-1f, MathF.Round(GeneCentroids[i].y+0.5f)-1f);
            // 切捨て
            //GeneCentroids[i] = new Vector2(MathF.Floor(GeneCentroids[i].x+0.5f), MathF.Round(GeneCentroids[i].y+0.5f));
            // 切り上げ
            //GeneCentroids[i] = new Vector2(MathF.Ceiling(GeneCentroids[i].x + 0.5f-1f), MathF.Round(GeneCentroids[i].y + 0.5f)-1f);
        }

        // インスペクターで道のリストが見えるようにするための作業------
        Grids = new List<ChildArray>();

        for (int i = 0; (i) < GeneMapData.Height; (i)++)
        {
            Grids.Add(new ChildArray());
            Grids[i].childArray=new List<int>();

        }

        // Height->widthの順番
        // グリッド座標（マップ）をみえるようにする
        for (int y = 0; y < GeneMapData.Height; y++)
        {
            for (int x = 0; x < GeneMapData.Width; x++)
            {
                Grids[GeneMapData.Height-y-1].childArray.Add((int)GeneMapData.Grids[y, x]);
            }
        }
        //print(Grids.Count);
        //print(Grids[0].childArray.Count);

        // ここまで--------------------------------------------------

        // これで頂点の接続順番を取ってこれた
        mesh.GetTriangles(triangles, 0);

        print("GeneMapData.Height:" + GeneMapData.Height);
        print("GeneMapData.Width:" + GeneMapData.Width);

        //print($"{nameof(Grids.Count)}:{Grids.Count}");
        //print($"{nameof(GeneTriangles.Count)}:{GeneTriangles.Count}");

        // 丸めてグリッド座標にしたものからマップデータの順番を取ってくる
        for (int i = 0; i < GeneCentroids.Count; i++)
        {
            int x = (int)GeneCentroids[i].x;
            int y = (int)GeneCentroids[i].y;

            RoundXY.Add(new Vector2Int(x, y));

            //print($"Grids[{y}].childArray.Count:{Grids[y].childArray.Count}");

            // 障害物の部分に-1を入れておく
            if (Grids[y].childArray[x] == (int)GridType.Obstacle)
            {
                int deleteTrianglesNum = i * 3;

                triangles[deleteTrianglesNum] = -1;
                triangles[deleteTrianglesNum + 1] = -1;
                triangles[deleteTrianglesNum + 2] = -1;

                if (isDebugLog)
                {
                    print("ssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss");

                    print($"kaisu(i):{i}");
                    print($"{nameof(x)}:{x}");
                    print($"{nameof(y)}:{y}");

                    print($"{nameof(deleteTrianglesNum)}*3:{deleteTrianglesNum}");
                    print($"{nameof(deleteTrianglesNum)}*3:{deleteTrianglesNum + 1}");
                    print($"{nameof(deleteTrianglesNum)}*3:{deleteTrianglesNum + 2}");

                    print("gggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg");

                }
            }
        }

        // -1が入っている物（障害物）を消す処理
        if (isDelete)
        {
            triangles.RemoveAll(x => x == -1);
            mesh.SetTriangles(triangles, 0);
            meshf.mesh = mesh;
        }

    }
}