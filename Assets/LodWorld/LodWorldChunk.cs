using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class LodWorldChunk
{
    public GameObject gameObject;
    public bool needsDrawn;
    public int chunkLodLevel = 1; // should never be set to zero

    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;
    private Mesh m_mesh;

    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<int> m_vertexIndex;
    private NativeArray<int> m_triangleIndex;

    private JobHandle m_handle;
    private LodChunkJob m_chunkJob;

    public LodWorldChunk(Material m_material, Vector3 m_position)
    {
        // Debug.Log("new chunk: " + m_position);

        m_mesh = new Mesh();

        needsDrawn = true;

        gameObject = new GameObject();
        gameObject.transform.position = m_position;

        m_meshFilter = gameObject.AddComponent<MeshFilter>();
        m_meshFilter.mesh = m_mesh;

        m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
        m_meshRenderer.material = m_material;
    }

    public void CheckLod(Vector3 centerPos, int lodIncrement)
    {
        // TODO Possibly Distance function in the LodChunkJob to increase performance
        int unitDistance = Mathf.RoundToInt(Vector3.Distance(centerPos, gameObject.transform.position) / Data.chunkSize) + 1;

        // check if lod level needs updated
        if (chunkLodLevel != unitDistance)
        {
            Debug.Log("Setting LOD: " + unitDistance);
            chunkLodLevel = unitDistance;
            needsDrawn = true;
        }
    }

    public void StartDraw()
    {
        if (needsDrawn)
        {
            // Debug.Log("Starting draw: " + gameObject.transform.position);

            m_vertices = new NativeArray<Vector3>(24 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.TempJob);
            m_triangles = new NativeArray<int>(36 * Data.chunkSize * Data.chunkSize * Data.chunkSize / 2, Allocator.TempJob);
            m_vertexIndex = new NativeArray<int>(1, Allocator.TempJob);
            m_triangleIndex = new NativeArray<int>(1, Allocator.TempJob);

            m_chunkJob = new LodChunkJob();
            m_chunkJob.lodLevel = chunkLodLevel;
            m_chunkJob.chunkPos = gameObject.transform.position;
            m_chunkJob.vertices = m_vertices;
            m_chunkJob.triangles = m_triangles;
            m_chunkJob.vertexIndex = m_vertexIndex;
            m_chunkJob.triangleIndex = m_triangleIndex;

            m_handle = m_chunkJob.Schedule();
        }
    }

    public void CompleteDraw()
    {
        if (needsDrawn)
        {
            // Debug.Log("Completing draw: " + gameObject.transform.position);

            m_handle.Complete();

            m_mesh = new Mesh
            {
                vertices = m_vertices.Slice<Vector3>(0, m_chunkJob.vertexIndex[0]).ToArray(),
                triangles = m_triangles.Slice<int>(0, m_chunkJob.triangleIndex[0]).ToArray()
            };

            m_mesh.RecalculateBounds();
            m_mesh.RecalculateNormals();

            m_meshFilter.mesh = m_mesh;

            m_vertices.Dispose();
            m_triangles.Dispose();
            m_vertexIndex.Dispose();
            m_triangleIndex.Dispose();

            needsDrawn = false;
        }
    }
}
