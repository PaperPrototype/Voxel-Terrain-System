// *********************
// NOT CURRENTLY WORKING
// *********************

using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class JobColorChunk : MonoBehaviour
{
    [SerializeField]
    private VoxelType[] voxelTypes;

    private NativeArray<VoxelType> m_voxelTypes;
    private NativeArray<Color> m_vertexColors;

    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<int> m_vertexIndex;
    private NativeArray<int> m_triangleIndex;

    private void Start()
    {
        m_voxelTypes = new NativeArray<VoxelType>(voxelTypes.Length, Allocator.Temp);
        m_vertexColors = new NativeArray<Color>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.Temp);

        m_vertices = new NativeArray<Vector3>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.TempJob);
        m_triangles = new NativeArray<int>(36 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.TempJob);
        m_vertexIndex = new NativeArray<int>(1, Allocator.TempJob);
        m_triangleIndex = new NativeArray<int>(1, Allocator.TempJob);

        m_voxelTypes.CopyFrom(voxelTypes);

        ColorChunkJob job = new ColorChunkJob();

        job.voxelTypes = m_voxelTypes;
        job.vertexColors = m_vertexColors;

        job.chunkPos = gameObject.transform.position;

        job.vertices = m_vertices;
        job.triangles = m_triangles;
        job.vertexIndex = m_vertexIndex;
        job.triangleIndex = m_triangleIndex;

        JobHandle handle = job.Schedule();
        handle.Complete();

        Mesh m_mesh = new Mesh
        {
            vertices = m_vertices.Slice<Vector3>(0, job.vertexIndex[0]).ToArray(),
            triangles = m_triangles.Slice<int>(0, job.triangleIndex[0]).ToArray()
        };

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh = m_mesh;

        // free memory
        m_voxelTypes.Dispose();
        m_vertexColors.Dispose();

        m_vertices.Dispose();
        m_triangles.Dispose();
        m_vertexIndex.Dispose();
        m_triangleIndex.Dispose();
    }
}


public struct ColorChunkJob : IJob
{
    public NativeArray<VoxelType> voxelTypes;
    public NativeArray<Color> vertexColors;

    public Vector3 chunkPos;

    public NativeArray<Vector3> vertices;
    public NativeArray<int> triangles;
    public NativeArray<int> vertexIndex;
    public NativeArray<int> triangleIndex;

    public void Execute()
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        vertexIndex[0] = 0;
        triangleIndex[0] = 0;

        for (int x = 0; x < Data.chunkSize; x++)
        {
            for (int y = 0; y < Data.chunkSize; y++)
            {
                for (int z = 0; z < Data.chunkSize; z++)
                {
                    Vector3 position = new Vector3(x, y, z);

                    if (IsSolid(noise, position))
                    {
                        DrawVoxel(noise, position);
                    }
                }
            }
        }
    }

    private void DrawVoxel(FastNoiseLite noise, Vector3 position)
    {
        for (int face = 0; face < 6; face++)
        {
            if (!IsSolid(noise, Data.NeighborOffset[face] + position))
            {
                vertices[vertexIndex[0] + 0] = position + Data.Vertices[Data.BuildOrder[face, 0]];
                vertices[vertexIndex[0] + 1] = position + Data.Vertices[Data.BuildOrder[face, 1]];
                vertices[vertexIndex[0] + 2] = position + Data.Vertices[Data.BuildOrder[face, 2]];
                vertices[vertexIndex[0] + 3] = position + Data.Vertices[Data.BuildOrder[face, 3]];

                // get the correct triangle index
                triangles[triangleIndex[0] + 0] = vertexIndex[0] + 0;
                triangles[triangleIndex[0] + 1] = vertexIndex[0] + 1;
                triangles[triangleIndex[0] + 2] = vertexIndex[0] + 2;
                triangles[triangleIndex[0] + 3] = vertexIndex[0] + 2;
                triangles[triangleIndex[0] + 4] = vertexIndex[0] + 1;
                triangles[triangleIndex[0] + 5] = vertexIndex[0] + 3;

                // increment by 4 because we only added 4 vertices
                vertexIndex[0] += 4;

                // increment by 6 because we only added 6 ints (6 / 3 = 2 triangles)
                triangleIndex[0] += 6;
            }
        }
    }

    private VoxelType GetVoxel(FastNoiseLite noise, Vector3 voxelPos)
    {
        float height = noise.GetNoise(voxelPos.x, voxelPos.z) * Data.chunkSize;
        if (voxelPos.y <= height)
        {

            return voxelTypes[1]; // dirt in voxel lookup table
        }
        else
        {
            return voxelTypes[0]; // air in lookup table
        }
    }

    private bool IsSolid(FastNoiseLite noise, Vector3 voxelPos)
    {
        return GetVoxel(noise, voxelPos).isSolid;
    }
}