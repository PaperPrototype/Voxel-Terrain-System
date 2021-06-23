using UnityEngine;
using Unity.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Jobs;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class JobWorldChunk2
{
    public GameObject gameObject;

    private JobWorld2 m_owner;

    private bool m_needsDrawn;

    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<Vector2> m_uvs;
    private NativeArray<int> m_vertexIndex;
    private NativeArray<int> m_triangleIndex;
    private NativeArray<byte> m_data;
    private NativeArray<byte> m_filePath;

    private JobHandle m_drawHandle;
    private JobHandle m_calcHandle;
    private JobHandle m_saveHandle;
    private JobHandle m_loadHandle;

    private JobDefs.DrawDataJob m_drawJob;
    private JobDefs.CalcDataJob m_calcJob;
    private JobDefs.SaveDataJob m_saveJob;
    private JobDefs.LoadDataJob m_loadJob;

    private Mesh m_mesh;

    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;
    private MeshCollider m_meshCollider;

    private string m_worldSaveName;


    public JobWorldChunk2(string worldSaveName, JobWorld2 owner, Vector3 position, Material material)
    {
        m_worldSaveName = worldSaveName;

        // set the chunk owner
        m_owner = owner;

        // instantiate chunk GameObject and set position
        gameObject = new GameObject();
        gameObject.transform.position = position;

        // add mesh filter to chunk GameObject set the meshFilter's mesh as well as the mesh colliders mesh
        m_meshFilter = gameObject.AddComponent<MeshFilter>();
        m_meshFilter.mesh = m_mesh;

        m_meshCollider = gameObject.AddComponent<MeshCollider>();
        m_meshCollider.sharedMesh = m_mesh;

        m_meshRenderer = gameObject.AddComponent<MeshRenderer>();
        m_meshRenderer.material = material;
    }

    /// <summary>
    /// Schedule calculating the chunks data
    /// </summary>
    public void ScheduleCalc()
    {
        // generating chunk data
        Debug.Log("Scheduling calc job for " + gameObject.transform.position);

        m_data = new NativeArray<byte>(DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize, Allocator.TempJob);

        JobDefs.CalcDataJob job = new JobDefs.CalcDataJob();
        job.data = m_data;

        m_calcHandle = job.Schedule();
    }

    /// <summary>
    /// Complete calculating the chunk's data and add it to the world dictionary
    /// </summary>
    public void CompleteCalc()
    {
        // completing chunk data
        Debug.Log("Completing calc job for " + gameObject.transform.position);

        m_calcHandle.Complete();

        m_owner.worldData.Add(gameObject.transform.position, new DataDefs.ChunkData(m_data.ToArray()));

        m_data.Dispose();
    }

    public void ScheduleDraw()
    {
        Debug.Log("Scheduling draw job for " + gameObject.transform.position);

        m_vertices = new NativeArray<Vector3>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_triangles = new NativeArray<int>(36 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_uvs = new NativeArray<Vector2>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_vertexIndex = new NativeArray<int>(1, Allocator.TempJob);
        m_triangleIndex = new NativeArray<int>(1, Allocator.TempJob);

        //                                               \/ <-- use gameObjects position as lookup key for dictionary
        m_data = new NativeArray<byte>(m_owner.worldData[gameObject.transform.position].data, Allocator.TempJob);

        m_drawJob = new JobDefs.DrawDataJob
        {
            data = m_data,
            vertices = m_vertices,
            triangles = m_triangles,
            uvs = m_uvs,
            vertexIndex = m_vertexIndex,
            triangleIndex = m_triangleIndex
        };

        m_drawHandle = m_drawJob.Schedule();
    }

    public void CompleteDraw()
    {
        m_drawHandle.Complete();

        m_mesh = new Mesh
        {
            vertices = m_vertices.Slice<Vector3>(0, m_drawJob.vertexIndex[0]).ToArray(),
            triangles = m_triangles.Slice<int>(0, m_drawJob.triangleIndex[0]).ToArray(),
            uv = m_uvs.Slice<Vector2>(0, m_drawJob.vertexIndex[0]).ToArray()
        };

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();

        m_meshFilter.mesh = m_mesh;
        m_meshCollider.sharedMesh = m_mesh;

        // free memory
        m_vertices.Dispose();
        m_triangles.Dispose();
        m_uvs.Dispose();
        m_vertexIndex.Dispose();
        m_triangleIndex.Dispose();
        m_data.Dispose();

        Debug.Log("Completed draw job for " + gameObject.transform.position);
    }
}