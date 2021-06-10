using UnityEngine;
using Unity.Collections;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Jobs;

[Serializable]
public struct ChunkData
{
    public byte[] data;

    public ChunkData(byte[] data)
    {
        this.data = data;
    }
}

public struct CalcDataJob: IJob
{
    public Vector3 position;
    public NativeArray<byte> data;

    public void Execute()
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        for (int x = 0; x < Data.chunkSize; x++)
        {
            for (int y = 0; y < Data.chunkSize; y++)
            {
                for (int z = 0; z < Data.chunkSize; z++)
                {
                    data[Utils.GetIndex(x, y, z)] = GetPerlinVoxel(noise, x, y, z);
                }
            }
        }
    }

    private byte GetPerlinVoxel(FastNoiseLite noise, float x, float y, float z)
    {
        float height = (noise.GetNoise(x + position.x, z + position.z) + 1) / 2 * Data.chunkSize;

        if (y >= height)
        {
            return 0; // air
        }
        else
        {
            return 1; // solid (the only "voxelType")
        }
    }
}


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class EditableChunk : MonoBehaviour
{
    public ChunkData chunkData;

    private NativeArray<byte> m_data;
    private JobHandle m_dataJobHandle;

    private Mesh m_mesh;
    private FastNoiseLite m_noise;

    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<Vector2> m_uvs;

    private int m_vertexIndex;
    private int m_triangleIndex;

    private void Start()
    {
        chunkData = new ChunkData(new byte[Data.chunkSize * Data.chunkSize * Data.chunkSize]);

        m_noise = new FastNoiseLite();
        m_noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);


        if (LoadChunk())
        {
            DrawChunk();
        }
        else
        {
            ScheduleCalc();
            CompleteCalc();
            DrawChunk();
        }
    }

    /// <summary>
    /// Loads the chunks data from a file. 
    /// </summary>
    /// <returns>If the file doesn't exist returns false.</returns>
    public bool LoadChunk()
    {
        string chunkFile = Application.persistentDataPath + "/chunks/" + gameObject.transform.position + ".chunk";
        if (File.Exists(chunkFile))
        {
            // create formatter and get file access
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Open(chunkFile, FileMode.Open);

            // deserialize the data and set the chunks data to be this data
            chunkData = (ChunkData)formatter.Deserialize(fileStream);
            fileStream.Close();

            return true;
        }
        return false;
    }

    public void SaveChunk()
    {
        string chunkFile = Application.persistentDataPath + "/chunks/" + gameObject.transform.position + ".chunk";
        if (!File.Exists(chunkFile))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(chunkFile));
        }
        // create formatter and create file access
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream fileStream = File.Open(chunkFile, FileMode.OpenOrCreate);

        // save the data and close the file access
        formatter.Serialize(fileStream, chunkData);
        fileStream.Close();
    }

    private void CalcChunkData()
    {
        for (int x = 0; x < Data.chunkSize; x++)
        {
            for (int y = 0; y < Data.chunkSize; y++)
            {
                for (int z = 0; z < Data.chunkSize; z++)
                {
                    chunkData.data[Utils.GetIndex(x, y, z)] = GetPerlinVoxel(x, y, z);
                }
            }
        }
    }

    private void ScheduleCalc()
    {
        m_data = new NativeArray<byte>(Data.chunkSize * Data.chunkSize * Data.chunkSize, Allocator.TempJob);

        CalcDataJob job = new CalcDataJob();
        job.data = m_data;

        m_dataJobHandle = job.Schedule();
    }

    private void CompleteCalc()
    {
        m_dataJobHandle.Complete();

        chunkData.data = m_data.ToArray();

        m_data.Dispose();
    }

    public void DrawChunk()
    {
        m_vertexIndex = 0;
        m_triangleIndex = 0;

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
        gameObject.GetComponent<MeshCollider>().sharedMesh = m_mesh;

        m_vertices.Dispose();
        m_triangles.Dispose();
        m_uvs.Dispose();
    }

    private void DrawVoxel(int x, int y, int z)
    {
        for (int side = 0; side < 6; side++)
        {
            if (!IsSolid(Data.NeighborOffset[side].x + x, Data.NeighborOffset[side].y + y, Data.NeighborOffset[side].z + z))
            {
                Vector3 pos = new Vector3(x, y, z);

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

    private byte GetPerlinVoxel(float x, float y, float z)
    {
        float height = (m_noise.GetNoise(x + gameObject.transform.position.x, z + gameObject.transform.position.z) + 1) / 2 * Data.chunkSize;

        if (y >= height)
        {
            return 0; // air
        }
        else
        {
            return 1; // solid (the only "voxelType")
        }
    }

    private bool IsSolid(int x, int y, int z)
    {
        // if inside bounds of data
        if (x >= 0 && x < Data.chunkSize &&
            y >= 0 && y < Data.chunkSize &&
            z >= 0 && z < Data.chunkSize)
        {
            byte voxelType = chunkData.data[Utils.GetIndex(x, y, z)];

            if (voxelType == 0) return false;
            else return true;
        }
        else
        {
            // this is where we check for neighbor chunks
            return false;
        }
    }

}