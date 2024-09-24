using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OptimizedGridMeshGenerator : MonoBehaviour
{
    // グリッドの状態を定義するenum
    public enum CellType { Empty, Obstacle }

    public int gridSizeX = 10; // X方向のグリッドの大きさ
    public int gridSizeZ = 10; // Z方向のグリッドの大きさ
    public float cellSize = 1f; // グリッドの1セルの大きさ

    public CellType[,] grid; // グリッドの各セルの状態を保持する配列

    private Mesh mesh;

    void Start()
    {
        Debug.Log("a");
        // グリッドを初期化（ランダムにEmptyとObstacleを割り当て）
        grid = new CellType[gridSizeX, gridSizeZ];
        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                grid[x, z] = (Random.value > 0.2f) ? CellType.Empty : CellType.Obstacle; // 20%の確率で障害物
            }
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GenerateOptimizedMesh();
    }

    void GenerateOptimizedMesh()
    {
        // 必要最低限の頂点と三角形を作成するためのリスト
        var vertices = new System.Collections.Generic.List<Vector3>();
        var triangles = new System.Collections.Generic.List<int>();

        // 頂点インデックスの追跡
        var vertexDict = new System.Collections.Generic.Dictionary<Vector3, int>();

        // グリッドの各セルを調べ、連続した「空いているセル」をグループ化してメッシュを生成
        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (grid[x, z] == CellType.Empty)
                {
                    // 各空白セルについて、そのセルの4つの頂点を作成/取得
                    Vector3 bottomLeft = new Vector3(x * cellSize, 0, z * cellSize);
                    Vector3 bottomRight = new Vector3((x + 1) * cellSize, 0, z * cellSize);
                    Vector3 topLeft = new Vector3(x * cellSize, 0, (z + 1) * cellSize);
                    Vector3 topRight = new Vector3((x + 1) * cellSize, 0, (z + 1) * cellSize);

                    // 重複する頂点を避けるため、辞書を使って頂点を追跡
                    int bl = AddVertex(vertices, vertexDict, bottomLeft);
                    int br = AddVertex(vertices, vertexDict, bottomRight);
                    int tl = AddVertex(vertices, vertexDict, topLeft);
                    int tr = AddVertex(vertices, vertexDict, topRight);

                    // 2つの三角形を作成
                    triangles.Add(bl);
                    triangles.Add(tl);
                    triangles.Add(br);

                    triangles.Add(br);
                    triangles.Add(tl);
                    triangles.Add(tr);
                }
            }
        }

        // メッシュにデータを適用
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    // 頂点を追加するヘルパーメソッド。すでに存在する場合は、そのインデックスを返す
    int AddVertex(System.Collections.Generic.List<Vector3> vertices, System.Collections.Generic.Dictionary<Vector3, int> vertexDict, Vector3 vertex)
    {
        if (vertexDict.ContainsKey(vertex))
        {
            return vertexDict[vertex];
        }
        else
        {
            vertices.Add(vertex);
            int index = vertices.Count - 1;
            vertexDict[vertex] = index;
            return index;
        }
    }

    // エディタで障害物の表示用にGizmosを描画
    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (grid[x, z] == CellType.Obstacle)
                {
                    Gizmos.color = Color.red;
                    Vector3 position = new Vector3(x * cellSize + cellSize / 2, 0, z * cellSize + cellSize / 2);
                    Gizmos.DrawCube(position, Vector3.one * cellSize * 0.9f); // 障害物を赤いキューブで表示
                }
            }
        }
    }
}