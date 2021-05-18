using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    private Mesh m_mesh;
    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private int m_vertexIndex = 0;
    private int m_triangleIndex = 0;

    private void Start()
    {
        m_vertices = new NativeArray<Vector3>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize, Allocator.Temp);
        m_triangles = new NativeArray<int>(36 * Data.chunkSize * Data.chunkSize * Data.chunkSize, Allocator.Temp);

        for (int x = 0; x < Data.chunkSize; x++)
        {
            for (int y = 0; y < Data.chunkSize; y++)
            {
                for (int z = 0; z < Data.chunkSize; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    DrawVoxel(pos);
                }
            }
        }

        m_mesh = new Mesh
        {
            vertices = m_vertices.ToArray(),
            triangles = m_triangles.ToArray()
        };

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = m_mesh;

        m_vertices.Dispose();
        m_triangles.Dispose();
    }

    private void DrawVoxel(Vector3 voxelPos)
    {
        for (int side = 0; side < 6; side++)
        {
            m_vertices[m_vertexIndex + 0] = Data.Vertices[Data.BuildOrder[side, 0]] + voxelPos;
            m_vertices[m_vertexIndex + 1] = Data.Vertices[Data.BuildOrder[side, 1]] + voxelPos;
            m_vertices[m_vertexIndex + 2] = Data.Vertices[Data.BuildOrder[side, 2]] + voxelPos;
            m_vertices[m_vertexIndex + 3] = Data.Vertices[Data.BuildOrder[side, 3]] + voxelPos;

            // get the correct triangle index
            m_triangles[m_triangleIndex + 0] = m_vertexIndex + 0;
            m_triangles[m_triangleIndex + 1] = m_vertexIndex + 1;
            m_triangles[m_triangleIndex + 2] = m_vertexIndex + 2;
            m_triangles[m_triangleIndex + 3] = m_vertexIndex + 2;
            m_triangles[m_triangleIndex + 4] = m_vertexIndex + 1;
            m_triangles[m_triangleIndex + 5] = m_vertexIndex + 3;

            // increment by 4 because we only added 4 vertices
            m_vertexIndex += 4;

            // increment by 6 because we added 6 int's to our triangles array
            m_triangleIndex += 6;
        }
    }

    //private bool IsSolid(Vector3 voxelPos)
    //{
    //    float height = m_noise.GetNoise(voxelPos.x, voxelPos.z) * Data.chunkSize;

    //    if (voxelPos.y <= height)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
}
