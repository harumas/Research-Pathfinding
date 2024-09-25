using System.Collections.Generic;
using UnityEngine;
using Visualizer.MapEditor;

public class Starter : MonoBehaviour
{
    [SerializeField] private MapDataManager mapDataManager;
    [SerializeField] private Material material;
    [SerializeField] private Vector2 displaySize;

    private void Awake()
    {
        MapData mapData = mapDataManager.Load();

        // ポリゴンの頂点を作成
        Vector2[] vertices = CreateVertices(mapData);
        Debug.Log(vertices.Length);

        // メッシュの作成
        var triangulator = new Triangulator();
        GameObject polygonObject = triangulator.CreateInfluencePolygon(vertices);

        // マテリアルの設定
        var meshRenderer = polygonObject.GetComponent<MeshRenderer>();
        meshRenderer.material = material;

        // 表示サイズの設定
        var displayScaler = polygonObject.AddComponent<DisplayScaler>();
        displayScaler.Scale(displaySize, new Vector2(mapData.Width, mapData.Height));
    }

    private Vector2[] CreateVertices(MapData mapData)
    {
        List<Vector2> vertices = new List<Vector2>();

        for (int y = 0; y < mapData.Height + 1; y++)
        {
            for (int x = 0; x < mapData.Width + 1; x++)
            {
                bool isLeftDown = y == 0 && x == 0;
                bool isLeftTop = y == mapData.Height && x == 0;
                bool isRightDown = y == 0 && x == mapData.Width;
                bool isRightTop = y == mapData.Height && x == mapData.Width;

                // 四つ角は頂点を作成
                if (isLeftDown || isLeftTop || isRightDown || isRightTop)
                {
                    vertices.Add(new Vector2(x, y));
                    continue;
                }

                byte corners = 0;

                // 左下
                if (y > 0 && x > 0)
                {
                    if ((mapData.Grids[y - 1, x - 1] & GridType.Obstacle) != 0)
                    {
                        corners |= 0b0001;
                    }
                }

                // 左上
                if (y < mapData.Height && x > 0)
                {
                    if ((mapData.Grids[y, x - 1] & GridType.Obstacle) != 0)
                    {
                        corners |= 0b0010;
                    }
                }

                // 右上
                if (y < mapData.Height && x < mapData.Width)
                {
                    if ((mapData.Grids[y, x] & GridType.Obstacle) != 0)
                    {
                        corners |= 0b0100;
                    }
                }

                // 右下
                if (y > 0 && x < mapData.Width)
                {
                    if ((mapData.Grids[y - 1, x] & GridType.Obstacle) != 0)
                    {
                        corners |= 0b1000;
                    }
                }


                // 障害物に触れてない場合 or 角じゃない場合は頂点を作成しない
                if (corners == 0b0000 || corners == 0b0011 || corners == 0b0110 || corners == 0b1100 || corners == 0b1001)
                {
                    continue;
                }

                vertices.Add(new Vector2(x, y));
            }
        }

        return vertices.ToArray();
    }
}