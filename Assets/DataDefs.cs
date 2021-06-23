using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;
using Unity.Jobs;
using Unity.Collections;
using System.Text;

public static class DataDefs
{
    public const int chunkNum = 15;

    public const int chunkSize = 16;

    public static readonly Vector3[] Vertices = new Vector3[8]
    {
        new Vector3(-0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, -0.5f, -0.5f),
        new Vector3(0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, 0.5f, -0.5f),
        new Vector3(-0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, -0.5f, 0.5f),
        new Vector3(0.5f, 0.5f, 0.5f),
        new Vector3(-0.5f, 0.5f, 0.5f),
    };

    public static readonly int[,] BuildOrder = new int[6, 4]
    {
        // right, left, up, down, front, back

        // 0 1 2 2 1 3 <- triangle order
        
        {1, 2, 5, 6}, // right face
        {4, 7, 0, 3}, // left face
        
        {3, 7, 2, 6}, // up face
        {1, 5, 0, 4}, // down face
        
        {5, 6, 4, 7}, // front face
        {0, 3, 1, 2}, // back face
    };

    public static readonly int3[] NeighborOffset = new int3[6]
    {
        new int3(1, 0, 0),  // right
        new int3(-1, 0, 0), // left
        new int3(0, 1, 0),  // up
        new int3(0, -1, 0), // down
        new int3(0, 0, 1),  // front
        new int3(0, 0, -1), // back
    };

    public static readonly Vector2[] UVs = new Vector2[4]
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(1, 0),
        new Vector2(1, 1),
    };


    [Serializable]
    public class ChunkData
    {
        public byte[] data;

        private NativeArray<byte> m_data;
        private NativeArray<byte> m_filePath;
        private JobHandle m_handle;

        public ChunkData(byte[] data)
        {
            this.data = data;
        }

        public void ScheduleSave(Vector3 position)
        {
            m_data = new NativeArray<byte>(data, Allocator.TempJob);
            m_filePath = new NativeArray<byte>(Encoding.ASCII.GetBytes(position.ToString().ToCharArray()), Allocator.TempJob);

            JobDefs.SaveDataJob job = new JobDefs.SaveDataJob()
            {
                data = m_data,
                filePath = m_filePath
            };


            m_handle = job.Schedule();
        }

        public void CompleteSave()
        {

        }
    }

}
