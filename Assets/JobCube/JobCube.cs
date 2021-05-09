using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class JobCube : MonoBehaviour
{
    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<int> m_vertexIndex;
    private NativeArray<int> m_triangleIndex;

    private void Start()
    {
        m_vertices = new NativeArray<Vector3>(24, Allocator.TempJob);
        m_triangles = new NativeArray<int>(36, Allocator.TempJob);
        m_vertexIndex = new NativeArray<int>(1, Allocator.TempJob);
        m_triangleIndex = new NativeArray<int>(1, Allocator.TempJob);

        CubeJob job = new CubeJob();
        job.vertexIndex = m_vertexIndex;
        job.triangleIndex = m_triangleIndex;
        job.vertices = m_vertices;
        job.triangles = m_triangles;

        JobHandle handle = job.Schedule();
        handle.Complete();

        print(job.vertexIndex[0]);
        print(job.triangleIndex[0]);
        print(m_vertices);
        print(m_triangles);

        Mesh m_mesh = new Mesh
        {
            // Todo use job.vertexIndex to get slice of num of vertices
            vertices = m_vertices.ToArray(),
            triangles = m_triangles.ToArray()
        };

        // free memory
        m_vertices.Dispose();
        m_triangles.Dispose();
        m_vertexIndex.Dispose();
        m_triangleIndex.Dispose();

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = m_mesh;
    }
}

public struct CubeJob : IJob
{
    public NativeArray<Vector3> vertices;
    public NativeArray<int> triangles;
    public NativeArray<int> vertexIndex;
    public NativeArray<int> triangleIndex;

    public void Execute()
    {
        vertexIndex[0] = 0;
        triangleIndex[0] = 0;
        DrawVoxel(0, 0, 0);
    }

    private void DrawVoxel(int x, int y, int z)
    {
        Vector3 position = new Vector3(x, y, z);

        for (int face = 0; face < 6; face++)
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
