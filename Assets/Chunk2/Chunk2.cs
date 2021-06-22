using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class Chunk2 : MonoBehaviour
{
    public byte[] data;
    public bool needsDrawn;
    public bool needsSaved;

    private NativeArray<Vector3> m_vertices;
    private NativeArray<int> m_triangles;
    private NativeArray<Vector2> m_uvs;

    private Mesh m_mesh;
    private FastNoiseLite m_noise;

    private int m_vertexIndex;
    private int m_triangleIndex;

    private void Start()
    {
        data = new byte[DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize];
        needsDrawn = false;
        needsSaved = false;

        m_noise = new FastNoiseLite();
        m_noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);


        // if the chunk was not loaded and LoadChunk returned false
        if (LoadChunk() == false)
        {
            // if no chunk data was loaded we need to calc the data ourselves
            CalcChunkData();
        }

        // finally we can draw the chunk's mesh
        DrawChunk();
    }

    private void Update()
    {
        if (needsDrawn == true)
        {
            print("needs drawn was true");
            DrawChunk();
            needsDrawn = false;
        }
    }

    private void OnDisable()
    {
        if (needsSaved == true)
        {
            SaveChunk();
        }
    }

    public void SaveChunk()
    {
        string filePath = Application.persistentDataPath + "/chunks/" + gameObject.transform.position + ".chunk";

        // check if folders and directory exist
        if (!File.Exists(filePath))
        {
            // ... if not create the directory
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        }

        // create formatter and get file access
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream fileStream = File.Open(filePath, FileMode.OpenOrCreate);

        // save the data through the fileStream
        formatter.Serialize(fileStream, data);

        // make sure to close the fileStream!
        fileStream.Close();

        print("the chunk saved to: " + filePath);
    }

    public bool LoadChunk()
    {
        string filePath = Application.persistentDataPath + "/chunks/" + gameObject.transform.position + ".chunk";

        // if the file exists load it, else don't
        if (File.Exists(filePath))
        {
            // create formatter and get file access
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Open(filePath, FileMode.Open);

            // deserialize the data and set the chunks data to be this data
            // cast the deserialize to a byte[]
            data = (byte[])formatter.Deserialize(fileStream);
            fileStream.Close();

            print("the chunk loaded from: " + filePath);

            return true;
        }

        // there wasn't a file to load so we return false
        return false;
    }

    public void EditChunkData(Vector3 worldPosition, byte voxelType)
    {
        int3 gridIndex = new int3
            (
                Mathf.RoundToInt(worldPosition.x - gameObject.transform.position.x),
                Mathf.RoundToInt(worldPosition.y - gameObject.transform.position.y),
                Mathf.RoundToInt(worldPosition.z - gameObject.transform.position.z)
            );

        data[Utils.GetIndex(gridIndex.x, gridIndex.y, gridIndex.z)] = voxelType;

        needsDrawn = true;
        needsSaved = true;
    }

    private void CalcChunkData()
    {
        for (int x = 0; x < DataDefs.chunkSize; x++)
        {
            for (int y = 0; y < DataDefs.chunkSize; y++)
            {
                for (int z = 0; z < DataDefs.chunkSize; z++)
                {
                    data[Utils.GetIndex(x, y, z)] = GetPerlinVoxel(x, y, z);
                }
            }
        }
    }

    public void DrawChunk()
    {
        m_vertexIndex = 0;
        m_triangleIndex = 0;

        m_vertices = new NativeArray<Vector3>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.Temp);
        m_triangles = new NativeArray<int>(36 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.Temp);
        m_uvs = new NativeArray<Vector2>(24 * DataDefs.chunkSize * DataDefs.chunkSize * DataDefs.chunkSize / 2, Allocator.Temp);

        for (int x = 0; x < DataDefs.chunkSize; x++)
        {
            for (int y = 0; y < DataDefs.chunkSize; y++)
            {
                for (int z = 0; z < DataDefs.chunkSize; z++)
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
            if (!IsSolid(DataDefs.NeighborOffset[side].x + x, DataDefs.NeighborOffset[side].y + y, DataDefs.NeighborOffset[side].z + z))
            {
                Vector3 pos = new Vector3(x, y, z);

                m_vertices[m_vertexIndex + 0] = DataDefs.Vertices[DataDefs.BuildOrder[side, 0]] + pos;
                m_vertices[m_vertexIndex + 1] = DataDefs.Vertices[DataDefs.BuildOrder[side, 1]] + pos;
                m_vertices[m_vertexIndex + 2] = DataDefs.Vertices[DataDefs.BuildOrder[side, 2]] + pos;
                m_vertices[m_vertexIndex + 3] = DataDefs.Vertices[DataDefs.BuildOrder[side, 3]] + pos;

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
        float height = (m_noise.GetNoise(x + gameObject.transform.position.x, z + gameObject.transform.position.z) + 1) / 2 * DataDefs.chunkSize;

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
        if (x >= 0 && x < DataDefs.chunkSize &&
            y >= 0 && y < DataDefs.chunkSize &&
            z >= 0 && z < DataDefs.chunkSize)
        {
            byte voxelType = data[Utils.GetIndex(x, y, z)];

            if (voxelType == 0) return false;
            else return true;
        }
        else
        {
            // this is where we would check for neighbor chunks
            return false;
        }
    }
}