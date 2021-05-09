using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCube : MonoBehaviour
{
    private Vector3[] m_vertices = new Vector3[24];
    private int[] m_triangles = new int[36];
    private int m_vertexIndex = 0;
    private int m_triangleIndex = 0;

    private void Start()
    {
        DrawVoxel(0, 0, 0);

        Mesh m_mesh = new Mesh
        {
            vertices = m_vertices,
            triangles = m_triangles
        };

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        MeshFilter m_meshFilter = gameObject.AddComponent<MeshFilter>();
        m_meshFilter.mesh = m_mesh;
    }

    public void DrawVoxel(int x, int y, int z)
    {
        Vector3 position = new Vector3(x, y, z);

        for (int face = 0; face < 6; face++)
        {
            m_vertices[m_vertexIndex + 0] = position + Data.Vertices[Data.BuildOrder[face, 0]];
            m_vertices[m_vertexIndex + 1] = position + Data.Vertices[Data.BuildOrder[face, 1]];
            m_vertices[m_vertexIndex + 2] = position + Data.Vertices[Data.BuildOrder[face, 2]];
            m_vertices[m_vertexIndex + 3] = position + Data.Vertices[Data.BuildOrder[face, 3]];

            // get the correct triangle index
            m_triangles[m_triangleIndex + 0] = m_vertexIndex + 0;
            m_triangles[m_triangleIndex + 1] = m_vertexIndex + 1;
            m_triangles[m_triangleIndex + 2] = m_vertexIndex + 2;
            m_triangles[m_triangleIndex + 3] = m_vertexIndex + 2;
            m_triangles[m_triangleIndex + 4] = m_vertexIndex + 1;
            m_triangles[m_triangleIndex + 5] = m_vertexIndex + 3;

            // increment by 4 because we only added 4 vertices
            m_vertexIndex += 4;

            // increment by 6 because added 6 ints
            m_triangleIndex += 6;
        }
    }
}