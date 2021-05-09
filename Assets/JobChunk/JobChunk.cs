using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class JobChunk : MonoBehaviour
{
    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<int> m_vertexIndex;
    private NativeArray<int> m_triangleIndex;

    private void Start()
    {
        // initialize

        m_vertices = new NativeArray<Vector3>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize, Allocator.TempJob);
        m_triangles = new NativeArray<int>(36 * Data.chunkSize * Data.chunkSize * Data.chunkSize, Allocator.TempJob);
        m_vertexIndex = new NativeArray<int>(1, Allocator.TempJob);
        m_triangleIndex = new NativeArray<int>(1, Allocator.TempJob);

        ChunkJob job = new ChunkJob();
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

        MeshFilter filter = gameObject.AddComponent<MeshFilter>();
        filter.mesh = m_mesh;

        // free memory
        m_vertices.Dispose();
        m_triangles.Dispose();
        m_vertexIndex.Dispose();
        m_triangleIndex.Dispose();
    }

    private void Update()
    {
       // start 
    }

    private void LateUpdate()
    {
        // end
    }
}

public struct ChunkJob : IJob
{
    public Vector3 chunkPos;
    public NativeArray<Vector3> vertices;
    public NativeArray<int> triangles;
    public NativeArray<int> vertexIndex;
    public NativeArray<int> triangleIndex;

    public void Execute()
    {
        vertexIndex[0] = 0;
        triangleIndex[0] = 0;

        for (int x = 0; x < Data.chunkSize; x++)
        {
            for (int y = 0; y < Data.chunkSize; y++)
            {
                for (int z = 0; z < Data.chunkSize; z++)
                {
                    Vector3 position = new Vector3(x, y, z);

                    if (IsSolid(position))
                    {
                        DrawVoxel(position);
                    }
                }
            }
        }
    }

    private void DrawVoxel(Vector3 position)
    {
        for (int face = 0; face < 6; face++)
        {
            if (!IsSolid(Data.NeighborOffset[face] + position))
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

    private bool IsSolid(Vector3 position)
    {
        int worldX = (int)position.x + (int)chunkPos.x;
        int worldY = (int)position.y + (int)chunkPos.y;
        int worldZ = (int)position.z + (int)chunkPos.z;

        int baseSurfaceHeight = NoiseUtils.GenerateHeight(chunkPos.x + position.x, chunkPos.z + position.z, 128);

        // generate surface terrain
        if (worldY <= baseSurfaceHeight - 5)
            return true;
        else
            return false;
    }
}
