using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private Mesh _hexMesh;
    private List<Vector3> _vertices;
    private List<Int32> _triangles;
    private MeshCollider _meshCollider;
    private List<Color> _colors;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = _hexMesh = new Mesh();
        _meshCollider = gameObject.AddComponent<MeshCollider>();
        _hexMesh.name = "Hex Mesh";
        _vertices = new List<Vector3>();
        _triangles = new List<Int32>();
        _colors = new List<Color>();
    }

    public void Triangulate(HexCell[] cells)
    {
        _hexMesh.Clear();
        _vertices.Clear();
        _triangles.Clear();
        _colors.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        _hexMesh.vertices = _vertices.ToArray();
        _hexMesh.triangles = _triangles.ToArray();
        _hexMesh.colors = _colors.ToArray();

        _hexMesh.RecalculateNormals();
        _meshCollider.sharedMesh = _hexMesh;
    }

    void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);

        //Add base trinagle
        AddTriangle(center, v1, v2);
        AddTriangleColor();

        //Vector3 v3 = center + HexMetrics.GetFirstCorner(direction);
        //Vector3 v4 = center + HexMetrics.GetSecondCorner(direction);

        //Add triangle border
        //AddQuad(v1, v2, v3, v4);
        //AddQuadColor(Color.black);

        //Add particles
        AddParticles(center, direction, cell);
    }

    private void AddParticles(Vector3 center, HexDirection direction, HexCell cell)
    {
        var x = center.x;
        var y = center.y;
        var z = center.z;
        var curA = new Vector3(0, 0, 0);
        var curB = new Vector3(0, 0, 0);
        var step = new Vector3(0, 0, 0);
        var rows = HexMetrics.OUTER_RADIUS;
        var v1 = HexMetrics.GetFirstSolidCorner(direction);
        var v2 = HexMetrics.GetSecondSolidCorner(direction);

        for (var i = 0; i < HexMetrics.OUTER_RADIUS; i++)
        {
            //Draw outer trinagles
            switch (i)
            {
                case 0:
                    curA = new Vector3(x, y, z);
                    break;
                case 1:
                    curA = new Vector3(x, y, z) + v1 * HexMetrics.PARTICLE_SIZE;
                    break;
                default:
                    curA = step + v1 * HexMetrics.PARTICLE_SIZE;
                    break;
            }

            for (var o = 0; o < rows; o++)
            {

                var a = curA;
                var b = curA + v1 * HexMetrics.PARTICLE_SIZE;
                var c = curA + v2 * HexMetrics.PARTICLE_SIZE;

                AdjustHeight(a, b, c, ref cell);

                curA = curA + v2 * HexMetrics.PARTICLE_SIZE;
            }

            //Draw covers
            switch (i)
            {
                case 0:
                    step = new Vector3(x, y, z);
                    curA = step + v2 * HexMetrics.PARTICLE_SIZE;
                    curB = step + v1 * HexMetrics.PARTICLE_SIZE;
                    break;
                default:
                    step = step + v1 * HexMetrics.PARTICLE_SIZE;
                    curA = step + v2 * HexMetrics.PARTICLE_SIZE;
                    curB = step + v1 * HexMetrics.PARTICLE_SIZE;
                    break;
            }

            for (var j = 0; j < rows - 1; j++)
            {
                var a = curA;
                var b = curB;
                var c = curA + v1 * HexMetrics.PARTICLE_SIZE;

                AdjustHeight(a, b, c, ref cell);

                curA = curA + v2 * HexMetrics.PARTICLE_SIZE;
                curB = curB + v2 * HexMetrics.PARTICLE_SIZE;
            }

            rows--;
        }
    }

    private void AdjustHeight(Vector3 v1, Vector3 v2, Vector3 v3, ref HexCell cell)
    {
        v1.y = Terrain.activeTerrain.SampleHeight(v1);
        v2.y = Terrain.activeTerrain.SampleHeight(v2);
        v3.y = Terrain.activeTerrain.SampleHeight(v3);

        var maxH = new List<float> { v1.y, v2.y, v3.y }.Max();
        if (maxH > cell.Height) cell.Height = maxH;
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var vertexIndex = _vertices.Count;
        _vertices.Add(v1);
        _vertices.Add(v2);
        _vertices.Add(v3);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
    }

    void AddTriangleColor()
    {
        var color = Color.white;
        _colors.Add(color);
        _colors.Add(color);
        _colors.Add(color);
    }

    void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        _colors.Add(c1);
        _colors.Add(c2);
        _colors.Add(c3);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        v1.y = Terrain.activeTerrain.SampleHeight(v1);
        v2.y = Terrain.activeTerrain.SampleHeight(v2);
        v3.y = Terrain.activeTerrain.SampleHeight(v3);
        v4.y = Terrain.activeTerrain.SampleHeight(v4);

        int vertexIndex = _vertices.Count;
        _vertices.Add(v1);
        _vertices.Add(v2);
        _vertices.Add(v3);
        _vertices.Add(v4);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 3);
    }

    void AddQuadColor(Color color)
    {
        _colors.Add(color);
        _colors.Add(color);
        _colors.Add(color);
        _colors.Add(color);
    }
}