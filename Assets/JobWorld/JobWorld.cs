using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobWorld : MonoBehaviour
{
    public Material material;
    public Transform center;
    public const int offset = Data.worldSize / 2;

    private JobWorldChunk[,] chunks = new JobWorldChunk[Data.worldSize, Data.worldSize];

    private void Start()
    {
        // initialize 256 chunks
        for (int x = 0; x < Data.worldSize; x++)
        {
            for (int z = 0; z < Data.worldSize; z++)
            {
                chunks[x, z] = new JobWorldChunk(material, new Vector3(x * Data.chunkSize, 0, z * Data.chunkSize));
            }
        }

        // 256 chunks drawing in jobs
        for (int x = 0; x < Data.worldSize; x++)
        {
            for (int z = 0; z < Data.worldSize; z++)
            {
                chunks[x, z].StartDraw();
            }
        }

        // 256 chunks finishing drawing
        for (int x = 0; x < Data.worldSize; x++)
        {
            for (int z = 0; z < Data.worldSize; z++)
            {
                chunks[x, z].CompleteDraw();
            }
        }
    }

    private void Update()
    {
        for (int x = 0; x < Data.worldSize; x++)
        {
            for (int z = 0; z < Data.worldSize; z++)
            {
                // x
                if (center.position.x + Data.worldSize * Data.chunkSize < chunks[x, z].gameObject.transform.position.x)
                {
                    chunks[x, z].gameObject.transform.position -= new Vector3(Data.worldSize * Data.chunkSize, 0, 0);
                    chunks[x, z].needsDrawn = true;
                }
                if (center.position.x > chunks[x, z].gameObject.transform.position.x)
                {
                    chunks[x, z].gameObject.transform.position += new Vector3(Data.worldSize * Data.chunkSize, 0, 0);
                    chunks[x, z].needsDrawn = true;
                }

                // z
                if (center.position.z + Data.worldSize * Data.chunkSize < chunks[x, z].gameObject.transform.position.z)
                {
                    chunks[x, z].gameObject.transform.position -= new Vector3(0, 0, Data.worldSize * Data.chunkSize);
                    chunks[x, z].needsDrawn = true;
                }
                if (center.position.z > chunks[x, z].gameObject.transform.position.z)
                {
                    chunks[x, z].gameObject.transform.position += new Vector3(0, 0, Data.worldSize * Data.chunkSize);
                    chunks[x, z].needsDrawn = true;
                }
            }
        }

        // 256 chunks drawing in jobs
        for (int x = 0; x < Data.worldSize; x++)
        {
            for (int z = 0; z < Data.worldSize; z++)
            {
                chunks[x, z].StartDraw();
            }
        }
    }

    private void LateUpdate()
    {
        // 256 chunks finishing drawing
        for (int x = 0; x < Data.worldSize; x++)
        {
            for (int z = 0; z < Data.worldSize; z++)
            {
                chunks[x, z].CompleteDraw();
            }
        }
    }
}
