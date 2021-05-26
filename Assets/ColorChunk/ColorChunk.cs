using System;
using UnityEngine;
using Unity.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ColorChunk : MonoBehaviour
{
    [SerializeField]
    private VoxelType[] voxelTypes;

    private Mesh m_mesh;
    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<Color> m_vertexColors;
    private int m_vertexIndex = 0;
    private int m_triangleIndex = 0;
    private FastNoiseLite m_noise;

    private void Start()
    {
        m_noise = new FastNoiseLite();
        m_noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        m_vertices = new NativeArray<Vector3>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.Temp);
        m_triangles = new NativeArray<int>(36 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.Temp);
        m_vertexColors = new NativeArray<Color>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.Temp);

        for (int x = 0; x < Data.chunkSize; x++)
        {
            for (int y = 0; y < Data.chunkSize; y++)
            {
                for (int z = 0; z < Data.chunkSize; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    if (IsSolid(pos))
                    {
                        DrawVoxel(pos);
                    }
                }
            }
        }

        m_mesh = new Mesh
        {
            vertices = m_vertices.Slice<Vector3>(0, m_vertexIndex).ToArray(),
            triangles = m_triangles.Slice<int>(0, m_triangleIndex).ToArray(),
            colors = m_vertexColors.Slice<Color>(0, m_vertexIndex).ToArray()
        };

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        gameObject.GetComponent<MeshFilter>().mesh = m_mesh;

        m_vertices.Dispose();
        m_triangles.Dispose();
        m_vertexColors.Dispose();
    }

    private void DrawVoxel(Vector3 voxelPos)
    {
        for (int side = 0; side < 6; side++)
        {
            if (!IsSolid(Data.NeighborOffset[side] + voxelPos))
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

                // get the color
                Color color = GetVoxel(voxelPos).color;

                // set the vertices colors
                m_vertexColors[m_vertexIndex + 0] = color;
                m_vertexColors[m_vertexIndex + 1] = color;
                m_vertexColors[m_vertexIndex + 2] = color;
                m_vertexColors[m_vertexIndex + 3] = color;

                // increment by 4 because we only added 4 vertices
                m_vertexIndex += 4;

                // increment by 6 because we added 6 int's to our triangles array
                m_triangleIndex += 6;
            }
        }
    }

    private VoxelType GetVoxel(Vector3 voxelPos)
    {
        float height = m_noise.GetNoise(voxelPos.x, voxelPos.z) * Data.chunkSize;

        if (voxelPos.y <= height)
        {
            return voxelTypes[1]; // dirt in voxel lookup table
        }
        else
        {
            return voxelTypes[0]; // air in lookup table
        }
    }

    private bool IsSolid(Vector3 voxelPos)
    {
        return GetVoxel(voxelPos).isSolid;
    }
}

[Serializable]
public struct VoxelType
{
    [SerializeField]
    public string name;

    [SerializeField]
    public Color color;

    [SerializeField]
    public bool isSolid;
}