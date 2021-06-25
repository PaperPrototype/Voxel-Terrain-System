using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public static class JobDefs
{
    public struct SaveDataJob : IJob
    {
        public NativeArray<byte> filePath;
        public NativeArray<byte> data;

        public void Execute()
        {
            string filePathStr = Encoding.ASCII.GetString(filePath.ToArray());

            if (!File.Exists(filePathStr))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePathStr));
            }

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Open(filePathStr, FileMode.OpenOrCreate);

            formatter.Serialize(fileStream, data.ToArray());

            fileStream.Close();
        }
    }

    public struct LoadDataJob : IJob
    {
        public NativeArray<byte> filePath;
        public NativeArray<byte> data;

        public void Execute()
        {
            string filePathStr = Encoding.ASCII.GetString(filePath.ToArray());

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = File.Open(filePathStr, FileMode.Open);

            byte[] tempdata = (byte[])formatter.Deserialize(fileStream);

            fileStream.Close();

            // put the byte[] from the file into the "data" NativeArray for access outside of the job
            data.CopyFrom(tempdata);
        }
    }
    
    public struct CalcDataJob : IJob
    {
        public Vector3 position;
        public NativeArray<byte> data;

        public void Execute()
        {
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            for (int x = 0; x < DataDefs.chunkSize; x++)
            {
                for (int y = 0; y < DataDefs.chunkSize; y++)
                {
                    for (int z = 0; z < DataDefs.chunkSize; z++)
                    {
                        data[Utils.GetIndex(x, y, z)] = Utils.GetPerlinVoxel(noise, position, x, y, z);
                    }
                }
            }
        }
    }

    public struct DrawDataJob : IJob
    {
        public NativeArray<byte> data;

        public NativeArray<Vector3> vertices;
        public NativeArray<int> triangles;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> vertexIndex;
        public NativeArray<int> triangleIndex;

        public void Execute()
        {
            vertexIndex[0] = 0;
            triangleIndex[0] = 0;

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
        }

        private void DrawVoxel(int x, int y, int z)
        {
            Vector3 pos = new Vector3(x, y, z);

            for (int face = 0; face < 6; face++)
            {
                if (!IsSolid(DataDefs.NeighborOffset[face].x + x, DataDefs.NeighborOffset[face].y + y, DataDefs.NeighborOffset[face].z + z))
                {
                    vertices[vertexIndex[0] + 0] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 0]];
                    vertices[vertexIndex[0] + 1] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 1]];
                    vertices[vertexIndex[0] + 2] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 2]];
                    vertices[vertexIndex[0] + 3] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 3]];

                    // get the correct triangle index
                    triangles[triangleIndex[0] + 0] = vertexIndex[0] + 0;
                    triangles[triangleIndex[0] + 1] = vertexIndex[0] + 1;
                    triangles[triangleIndex[0] + 2] = vertexIndex[0] + 2;
                    triangles[triangleIndex[0] + 3] = vertexIndex[0] + 2;
                    triangles[triangleIndex[0] + 4] = vertexIndex[0] + 1;
                    triangles[triangleIndex[0] + 5] = vertexIndex[0] + 3;

                    uvs[vertexIndex[0] + 0] = DataDefs.UVs[0];
                    uvs[vertexIndex[0] + 1] = DataDefs.UVs[1];
                    uvs[vertexIndex[0] + 2] = DataDefs.UVs[2];
                    uvs[vertexIndex[0] + 3] = DataDefs.UVs[3];

                    // increment by 4 because we only added 4 vertices
                    vertexIndex[0] += 4;

                    // increment by 6 because we only added 6 ints (6 / 3 = 2 triangles)
                    triangleIndex[0] += 6;
                }
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
                // this is where we check for neighbor chunks
                return false;
            }
        }
    }

    public struct ChunkJob : IJob
    {
        public Vector3 chunkPos;
        public NativeArray<Vector3> vertices;
        public NativeArray<int> triangles;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> vertexIndex;
        public NativeArray<int> triangleIndex;

        public void Execute()
        {
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            vertexIndex[0] = 0;
            triangleIndex[0] = 0;

            for (int x = 0; x < DataDefs.chunkSize; x++)
            {
                for (int y = 0; y < DataDefs.chunkSize; y++)
                {
                    for (int z = 0; z < DataDefs.chunkSize; z++)
                    {
                        if (IsSolid(noise, x, y, z))
                        {
                            DrawVoxel(noise, x, y, z);
                        }
                    }
                }
            }
        }

        private void DrawVoxel(FastNoiseLite noise, int x, int y, int z)
        {
            Vector3 pos = new Vector3(x, y, z);

            for (int face = 0; face < 6; face++)
            {
                if (!IsSolid(noise, DataDefs.NeighborOffset[face].x + x, DataDefs.NeighborOffset[face].y + y, DataDefs.NeighborOffset[face].z + z))
                {
                    vertices[vertexIndex[0] + 0] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 0]];
                    vertices[vertexIndex[0] + 1] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 1]];
                    vertices[vertexIndex[0] + 2] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 2]];
                    vertices[vertexIndex[0] + 3] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 3]];

                    // get the correct triangle index
                    triangles[triangleIndex[0] + 0] = vertexIndex[0] + 0;
                    triangles[triangleIndex[0] + 1] = vertexIndex[0] + 1;
                    triangles[triangleIndex[0] + 2] = vertexIndex[0] + 2;
                    triangles[triangleIndex[0] + 3] = vertexIndex[0] + 2;
                    triangles[triangleIndex[0] + 4] = vertexIndex[0] + 1;
                    triangles[triangleIndex[0] + 5] = vertexIndex[0] + 3;

                    uvs[vertexIndex[0] + 0] = new Vector2(0, 0);
                    uvs[vertexIndex[0] + 1] = new Vector2(0, 1);
                    uvs[vertexIndex[0] + 2] = new Vector2(1, 0);
                    uvs[vertexIndex[0] + 3] = new Vector2(1, 1);

                    // increment by 4 because we only added 4 vertices
                    vertexIndex[0] += 4;

                    // increment by 6 because we only added 6 ints (6 / 3 = 2 triangles)
                    triangleIndex[0] += 6;
                }
            }
        }

        private bool IsSolid(FastNoiseLite noise, int x, int y, int z)
        {
            float height = (noise.GetNoise(x + chunkPos.x, z + chunkPos.z) + 1) / 2 * DataDefs.chunkSize;

            if (y <= height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }


    public struct PlanetChunkJob : IJob
    {
        public float planetRadius;
        public Vector3 chunkPos;
        public NativeArray<Vector3> vertices;
        public NativeArray<int> triangles;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> vertexIndex;
        public NativeArray<int> triangleIndex;

        public void Execute()
        {
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

            vertexIndex[0] = 0;
            triangleIndex[0] = 0;

            for (int x = 0; x < DataDefs.chunkSize; x++)
            {
                for (int y = 0; y < DataDefs.chunkSize; y++)
                {
                    for (int z = 0; z < DataDefs.chunkSize; z++)
                    {
                        if (IsSolid(noise, x, y, z))
                        {
                            DrawVoxel(noise, x, y, z);
                        }
                    }
                }
            }
        }

        private void DrawVoxel(FastNoiseLite noise, int x, int y, int z)
        {
            Vector3 pos = new Vector3(x, y, z);

            for (int face = 0; face < 6; face++)
            {
                if (!IsSolid(noise, DataDefs.NeighborOffset[face].x + x, DataDefs.NeighborOffset[face].y + y, DataDefs.NeighborOffset[face].z + z))
                {
                    vertices[vertexIndex[0] + 0] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 0]];
                    vertices[vertexIndex[0] + 1] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 1]];
                    vertices[vertexIndex[0] + 2] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 2]];
                    vertices[vertexIndex[0] + 3] = pos + DataDefs.Vertices[DataDefs.BuildOrder[face, 3]];

                    // get the correct triangle index
                    triangles[triangleIndex[0] + 0] = vertexIndex[0] + 0;
                    triangles[triangleIndex[0] + 1] = vertexIndex[0] + 1;
                    triangles[triangleIndex[0] + 2] = vertexIndex[0] + 2;
                    triangles[triangleIndex[0] + 3] = vertexIndex[0] + 2;
                    triangles[triangleIndex[0] + 4] = vertexIndex[0] + 1;
                    triangles[triangleIndex[0] + 5] = vertexIndex[0] + 3;

                    uvs[vertexIndex[0] + 0] = new Vector2(0, 0);
                    uvs[vertexIndex[0] + 1] = new Vector2(0, 1);
                    uvs[vertexIndex[0] + 2] = new Vector2(1, 0);
                    uvs[vertexIndex[0] + 3] = new Vector2(1, 1);

                    // increment by 4 because we only added 4 vertices
                    vertexIndex[0] += 4;

                    // increment by 6 because we only added 6 ints (6 / 3 = 2 triangles)
                    triangleIndex[0] += 6;
                }
            }
        }

        private bool IsSolid(FastNoiseLite noise, int x, int y, int z)
        {
            float distance = Vector3.Distance(new Vector3(x, y, z) + chunkPos, Vector3.zero);

            if (distance <= planetRadius)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
