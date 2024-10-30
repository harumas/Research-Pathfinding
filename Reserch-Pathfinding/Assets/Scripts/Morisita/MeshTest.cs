using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using UnityEngine;
using UnityEngine.Rendering;
using Visualizer.MapEditor;


//�V���A���C�Y���ꂽ�q�v�f�N���X
[System.Serializable]
public class ChildArray
{
    public List<int> childArray;
}
public class MeshTest : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Mesh myMesh;
    [SerializeField] bool isDelete;
    [SerializeField] bool isDebugLog;

    [SerializeField]
    List<int> triangles = new List<int>();

    [SerializeField,Header("�d�S�̓s���ŉ��̍s��������")]
    List<ChildArray> Grids;
    [SerializeField]
    List<Vector2> OriginalXY = new List<Vector2>();
    [SerializeField]
    List<Vector2Int> RoundXY = new List<Vector2Int>();
     
    void Start()
    {

        meshFilter = GetComponent<MeshFilter>();
        myMesh = new Mesh();

        List<Vector3> myVertices = new List<Vector3>
        {
            new Vector3(-1, -1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, -1, 0)
        };

        // {0,1,2}�ň�ڂ̎O�p�`�A{0,2,3}�œ�ڂ̎O�p�`�B�Ȃ̂�{0,1,2,2,3,0}�ł������l�p�`���ł���
        List<int> myTriangles = new List<int> { 0, 1, 2, 0, 2, 3 };


        myMesh.SetVertices(myVertices);

        myMesh.SetTriangles(myTriangles, 0);

        //MeshFilter�ւ̊��蓖��
        //meshFilter.mesh = myMesh;
    }

    public void DeleteBlockMesh(M_GenerateContext context)
    {
        // �������񉽂������Ă��Ă�̂���u���Ă���
        // �������ꂽ�O�p�`�ɕ������ꂽ���b�V���I�u�W�F�N�g
        GameObject GeneObjext = context.GeneratedObject;

        // �O�p�`�̒��_���
        List<(Vector2 v0, Vector2 v2, Vector2 v3)> GeneTriangles = context.Triangles;

        // �O�p�`�̏d�S
        // ������ۂ߂ăO���b�h���W�ɂ��ă}�b�v�����Q�Ƃ���
        List<Vector2> GeneCentroids = context.Centroids;

        // �O���b�h�}�b�v�f�[�^
        MapData GeneMapData = context.MapData;

        MeshFilter meshf = GeneObjext.GetComponent<MeshFilter>();
        Mesh mesh = meshf.mesh;

        // �O�p�`�̏d�S�����邽�߂Ɏ��o��
        for (int i = 0; i < GeneCentroids.Count(); i++)
        {
            OriginalXY.Add(new Vector2(GeneCentroids[i].x,GeneCentroids[i].y));
        }

        // �O�p�`�̏d�S���ۂ߂�
        for (int i = 0; i < GeneCentroids.Count(); i++)
        {
            // �ۂ�
            GeneCentroids[i] = new Vector2(MathF.Round(GeneCentroids[i].x+0.5f)-1f, MathF.Round(GeneCentroids[i].y+0.5f)-1f);
            // �؎̂�
            //GeneCentroids[i] = new Vector2(MathF.Floor(GeneCentroids[i].x+0.5f), MathF.Round(GeneCentroids[i].y+0.5f));
            // �؂�グ
            //GeneCentroids[i] = new Vector2(MathF.Ceiling(GeneCentroids[i].x + 0.5f-1f), MathF.Round(GeneCentroids[i].y + 0.5f)-1f);


        }


        // �C���X�y�N�^�[�œ��̃��X�g��������悤�ɂ��邽�߂̍��
        Grids = new List<ChildArray>();

        for (int i = 0; (i) < GeneMapData.Height; (i)++)
        {
            Grids.Add(new ChildArray());
            Grids[i].childArray=new List<int>();

        }

        // Height->width�̏���
        // �O���b�h���W�i�}�b�v�j���݂���悤�ɂ���
        for (int y = 0; y < GeneMapData.Height; y++)
        {
            for (int x = 0; x < GeneMapData.Width; x++)
            {
                Grids[GeneMapData.Height-y-1].childArray.Add((int)GeneMapData.Grids[y, x]);
            }
        }
        print(Grids.Count);
        print(Grids[0].childArray.Count);

        // �����܂�


        // ����Œ��_�̐ڑ����Ԃ�����Ă��ꂽ
        mesh.GetTriangles(triangles, 0);


        print("GeneMapData.Height:" + GeneMapData.Height);
        print("GeneMapData.Width:" + GeneMapData.Width);

        //print($"{nameof(Grids.Count)}:{Grids.Count}");
        //print($"{nameof(GeneTriangles.Count)}:{GeneTriangles.Count}");

        // �ۂ߂ăO���b�h���W�ɂ������̂���}�b�v�f�[�^�̏��Ԃ�����Ă���
        for (int i = 0; i < GeneCentroids.Count; i++)
        {
            int x = (int)GeneCentroids[i].x;
            int y = (int)GeneCentroids[i].y;

            RoundXY.Add(new Vector2Int(x, y));

            //int Gridzahyou = x + y;

            //print($"Grids[{y}].childArray.Count:{Grids[y].childArray.Count}");

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

        //for(int i=0;i<)

        /// ��U�R�̔{���ɂȂ��Ă���Ɖ��肵�ď����Ă݂�@<-�_��������
        //for (int i = 0; i < Grids.Count(); i++)
        //{
        //    if (Grids[i] == 2)
        //    {
        //        int x = i * 3;
        //        triangles[x] = -1;
        //        triangles[x + 1] = -1;
        //        triangles[x + 2] = -1;
        //    }
        //}
        // ��������
        if (isDelete)
        {
            triangles.RemoveAll(x => x == -1);
            mesh.SetTriangles(triangles, 0);
            meshf.mesh = mesh;
        }

    }
}