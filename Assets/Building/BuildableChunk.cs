using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using Unity.Jobs;
using Unity.Collections;

public class BuildableChunk : MonoBehaviour
{
    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<Vector2> m_uvs;
    private NativeArray<int> m_vertexIndex;
    private NativeArray<int> m_triangleIndex;

    private void Start()
    {

        m_vertices = new NativeArray<Vector3>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_triangles = new NativeArray<int>(36 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_uvs = new NativeArray<Vector2>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_vertexIndex = new NativeArray<int>(1, Allocator.TempJob);
        m_triangleIndex = new NativeArray<int>(1, Allocator.TempJob);

        JobDefs.ChunkJob job = new JobDefs.ChunkJob();
        job.chunkPos = gameObject.transform.position;
        job.vertices = m_vertices;
        job.triangles = m_triangles;
        job.uvs = m_uvs;
        job.vertexIndex = m_vertexIndex;
        job.triangleIndex = m_triangleIndex;

        JobHandle handle = job.Schedule();
        handle.Complete();

        Mesh m_mesh = new Mesh
        {
            vertices = m_vertices.Slice<Vector3>(0, job.vertexIndex[0]).ToArray(),
            triangles = m_triangles.Slice<int>(0, job.triangleIndex[0]).ToArray(),
            uv = m_uvs.Slice<Vector2>(0, job.vertexIndex[0]).ToArray()
        };

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        filter.mesh = m_mesh;

        // free memory
        m_vertices.Dispose();
        m_triangles.Dispose();
        m_uvs.Dispose();
        m_vertexIndex.Dispose();
        m_triangleIndex.Dispose();
    }
}
