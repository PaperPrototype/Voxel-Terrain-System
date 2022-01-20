using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class SimplePlanetChunk
{
    public GameObject gameObject;
    public bool needsDrawn;

    private SimplePlanet m_owner;

    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;
    private MeshCollider m_meshCollider;
    private Mesh m_mesh;

    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<Vector2> m_uvs;
    private NativeArray<int> m_vertexIndex;
    private NativeArray<int> m_triangleIndex;

    private JobHandle m_handle;
    private JobDefs.PlanetChunkJob m_chunkJob;

    public SimplePlanetChunk(SimplePlanet owner, Material m_material, Vector3 m_position)
    {
        m_owner = owner;

        m_mesh = new Mesh();
        needsDrawn = true;

        gameObject = new GameObject();
        gameObject.transform.position = m_position;

        m_meshFilter = gameObject.AddComponent<MeshFilter>();
        m_meshCollider = gameObject.AddComponent<MeshCollider>();

        m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
        m_meshRenderer.material = m_material;
    }

    public void ScheduleDraw()
    {
        if (needsDrawn == true)
        {
            m_vertices = new NativeArray<Vector3>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
            m_triangles = new NativeArray<int>(36 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
            m_uvs = new NativeArray<Vector2>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
            m_vertexIndex = new NativeArray<int>(1, Allocator.TempJob);
            m_triangleIndex = new NativeArray<int>(1, Allocator.TempJob);

            m_chunkJob = new JobDefs.PlanetChunkJob();
            m_chunkJob.amplitude = m_owner.amplitude;
            m_chunkJob.frequency = m_owner.frequency;
            m_chunkJob.planetRadius = m_owner.planetRadius;
            m_chunkJob.chunkPos = gameObject.transform.position;
            m_chunkJob.vertices = m_vertices;
            m_chunkJob.triangles = m_triangles;
            m_chunkJob.uvs = m_uvs;
            m_chunkJob.vertexIndex = m_vertexIndex;
            m_chunkJob.triangleIndex = m_triangleIndex;

            m_handle = m_chunkJob.Schedule();
        }
    }

    public void CompleteDraw()
    {
        if (needsDrawn == true)
        {
            m_handle.Complete();

            m_mesh = new Mesh
            {
                vertices = m_vertices.Slice<Vector3>(0, m_chunkJob.vertexIndex[0]).ToArray(),
                triangles = m_triangles.Slice<int>(0, m_chunkJob.triangleIndex[0]).ToArray(),
                uv = m_uvs.Slice<Vector2>(0, m_chunkJob.vertexIndex[0]).ToArray()
            };

            m_mesh.RecalculateBounds();
            m_mesh.RecalculateNormals();

            m_meshFilter.mesh = m_mesh;
            m_meshCollider.sharedMesh = m_mesh;

            m_vertices.Dispose();
            m_triangles.Dispose();
            m_uvs.Dispose();
            m_vertexIndex.Dispose();
            m_triangleIndex.Dispose();

            needsDrawn = true;
        }
    }
}
