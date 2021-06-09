using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    private Mesh m_mesh;

    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<Vector2> m_uvs;
    private int m_vertexIndex = 0;
    private int m_triangleIndex = 0;

    private FastNoiseLite m_noise;

    private void Start()
    {
        m_noise = new FastNoiseLite();
        m_noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        m_vertices = new NativeArray<Vector3>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.Temp);
        m_triangles = new NativeArray<int>(36 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.Temp);
        m_uvs = new NativeArray<Vector2>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.Temp);

        for (int x = 0; x < Data.chunkSize; x++)
        {
            for (int y = 0; y < Data.chunkSize; y++)
            {
                for (int z = 0; z < Data.chunkSize; z++)
                {
                    if (IsSolid(x, y, z))
                    {
                        DrawVoxel(x, y, z);
                    }
                }
            }
        }

        m_mesh = new Mesh
        {
            vertices = m_vertices.Slice<Vector3>(0, m_vertexIndex).ToArray(),
            triangles = m_triangles.Slice<int>(0, m_triangleIndex).ToArray(),
            uv = m_uvs.Slice<Vector2>(0, m_vertexIndex).ToArray()
        };

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = m_mesh;

        m_vertices.Dispose();
        m_triangles.Dispose();
        m_uvs.Dispose();
    }

    private void DrawVoxel(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z);

        for (int side = 0; side < 6; side++)
        {
            if (!IsSolid(x + Data.NeighborOffset[side].x, y + Data.NeighborOffset[side].y, z + Data.NeighborOffset[side].z))
            {
                m_vertices[m_vertexIndex + 0] = Data.Vertices[Data.BuildOrder[side, 0]] + pos;
                m_vertices[m_vertexIndex + 1] = Data.Vertices[Data.BuildOrder[side, 1]] + pos;
                m_vertices[m_vertexIndex + 2] = Data.Vertices[Data.BuildOrder[side, 2]] + pos;
                m_vertices[m_vertexIndex + 3] = Data.Vertices[Data.BuildOrder[side, 3]] + pos;

                m_triangles[m_triangleIndex + 0] = m_vertexIndex + 0;
                m_triangles[m_triangleIndex + 1] = m_vertexIndex + 1;
                m_triangles[m_triangleIndex + 2] = m_vertexIndex + 2;
                m_triangles[m_triangleIndex + 3] = m_vertexIndex + 2;
                m_triangles[m_triangleIndex + 4] = m_vertexIndex + 1;
                m_triangles[m_triangleIndex + 5] = m_vertexIndex + 3;

                m_uvs[m_vertexIndex + 0] = new Vector2(0, 0);
                m_uvs[m_vertexIndex + 1] = new Vector2(0, 1);
                m_uvs[m_vertexIndex + 2] = new Vector2(1, 0);
                m_uvs[m_vertexIndex + 3] = new Vector2(1, 1);

                // increment by 4 because we only added 4 vertices
                m_vertexIndex += 4;

                // increment by 6 because we added 6 int's to our triangles array
                m_triangleIndex += 6;
            }
        }
    }
    
    private bool IsSolid(int x, int y, int z)
    {
        float height = (m_noise.GetNoise(x, z) + 1) / 2 * Data.chunkSize;

        if (y <= height)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}