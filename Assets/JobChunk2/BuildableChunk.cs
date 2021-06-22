using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class BuildableChunk : MonoBehaviour
{
    public ChunkData chunkData;

    private bool m_needsDrawn;
    public bool needsSaved;

    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<Vector2> m_uvs;
    private NativeArray<int> m_vertexIndex;
    private NativeArray<int> m_triangleIndex;
    private NativeArray<byte> m_data;
    private NativeArray<byte> m_filePath;

    private JobDefs.DrawDataJob m_drawJob;
    private JobDefs.CalcDataJob m_calcJob;
    private JobDefs.SaveDataJob m_saveJob;
    private JobDefs.LoadDataJob m_loadJob;

    private JobHandle m_drawHandle;
    private JobHandle m_calcHandle;
    private JobHandle m_saveHandle;
    private JobHandle m_loadHandle;

    private Mesh m_mesh;

    private MeshFilter m_meshFilter;
    private MeshCollider m_meshCollider;

    private void Update()
    {
        if (m_needsDrawn == true)
        {
            ScheduleDraw();
            CompleteDraw();

            m_needsDrawn = false;
        }
    }

    public void ChangeVoxel(Vector3 worldPos, byte voxelType)
    {
        int3 gridPosition = new int3(
                Mathf.RoundToInt(worldPos.x - gameObject.transform.position.x),
                Mathf.RoundToInt(worldPos.y - gameObject.transform.position.y),
                Mathf.RoundToInt(worldPos.z - gameObject.transform.position.z)
            );

        chunkData.data[Utils.GetIndex(gridPosition.x, gridPosition.y, gridPosition.z)] = voxelType;
        m_needsDrawn = true;
        needsSaved = true;
    }

    private void OnEnable()
    {
        needsSaved = false;
        m_needsDrawn = false;
        chunkData = new ChunkData(new byte[DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize]);

        m_meshFilter = gameObject.GetComponent<MeshFilter>();
        m_meshCollider = gameObject.GetComponent<MeshCollider>();

        // if no load was needed
        if (!ScheduleLoad())
        {
            ScheduleCalc();
            CompleteCalc();
        }
        // load was needed and job was run
        // now complete the job and set the data
        else
        {
            CompleteLoad();
        }

        ScheduleDraw();
        CompleteDraw();
    }

    private void OnDisable()
    {
        // if scheule save ran a save job...
        if (ScheduleSave())
        {
            // then complete that job
            CompleteSave();
        }
    }

    public string GetCurrentSaveName()
    {
        return Application.persistentDataPath + "/chunks/" + DataDefs.chunkSize + "-" + gameObject.transform.position + ".chunk";
    }

    public bool ScheduleSave()
    {
        if (needsSaved == true)
        {
            print("running save job for " + GetCurrentSaveName());
            m_data = new NativeArray<byte>(chunkData.data, Allocator.TempJob);
            m_filePath = new NativeArray<byte>(Encoding.ASCII.GetBytes(GetCurrentSaveName().ToCharArray()), Allocator.TempJob);

            m_saveJob = new JobDefs.SaveDataJob()
            {
                filePath = m_filePath,
                data = m_data
            };

            m_saveHandle = m_saveJob.Schedule();

            return true;
        }

        return false;
    }

    public void CompleteSave()
    {
        m_saveHandle.Complete();

        needsSaved = false;

        m_data.Dispose();
        m_filePath.Dispose();

        print("completed save job");
    }

    public bool ScheduleLoad()
    {
        if (File.Exists(GetCurrentSaveName()))
        {
            print("running load job for " + GetCurrentSaveName());

            m_data = new NativeArray<byte>(DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize, Allocator.TempJob);
            m_filePath = new NativeArray<byte>(Encoding.ASCII.GetBytes(GetCurrentSaveName().ToCharArray()), Allocator.TempJob);

            m_loadJob = new JobDefs.LoadDataJob()
            {
                filePath = m_filePath,
                data = m_data
            };

            m_loadHandle = m_loadJob.Schedule();

            return true;
        }

        return false;
    }

    public void CompleteLoad()
    {
        m_loadHandle.Complete();

        chunkData.data = m_data.ToArray();

        m_filePath.Dispose();
        m_data.Dispose();

        print("completed load job");
    }

    public bool TrySave()
    {
        if (needsSaved == true)
        {
            string fileName = GetCurrentSaveName();

            print(fileName);

            if (!File.Exists(fileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }

            // create formatter and create file access
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Open(fileName, FileMode.OpenOrCreate);

            // save the data and close the file access
            formatter.Serialize(fileStream, chunkData);
            fileStream.Close();

            return true;
        }

        return false;
    }

    public bool TryLoad()
    {
        string fileName = GetCurrentSaveName();

        // if there is a chunk file load it
        if (File.Exists(fileName))
        {
            // create formatter and get file access
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Open(fileName, FileMode.Open);

            // deserialize the data and set the chunks data to be this data
            // cast the deserialize to a ChunkData
            chunkData = (ChunkData)formatter.Deserialize(fileStream);

            fileStream.Close();

            return true;
        }

        return false;
    }


    public void ScheduleCalc()
    {
        print("rnning calc job for " + GetCurrentSaveName());

        m_data = new NativeArray<byte>(chunkData.data, Allocator.TempJob);

        m_calcJob = new JobDefs.CalcDataJob
        {
            data = m_data,
            position = gameObject.transform.position
        };

        m_calcHandle = m_calcJob.Schedule();
    }

    public void CompleteCalc()
    {
        m_calcHandle.Complete();

        chunkData.data = m_data.ToArray();

        m_data.Dispose();

        print("completed calc job");
    }

    public void ScheduleDraw()
    {
        print("running draw job for " + GetCurrentSaveName());

        m_vertices = new NativeArray<Vector3>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_triangles = new NativeArray<int>(36 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_uvs = new NativeArray<Vector2>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.TempJob);
        m_vertexIndex = new NativeArray<int>(1, Allocator.TempJob);
        m_triangleIndex = new NativeArray<int>(1, Allocator.TempJob);
        m_data = new NativeArray<byte>(chunkData.data, Allocator.TempJob);

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

        print("completed draw job");
    }

}

[Serializable]
public struct ChunkData
{
    public byte[] data;

    public ChunkData(byte[] data)
    {
        this.data = data;
    }
}