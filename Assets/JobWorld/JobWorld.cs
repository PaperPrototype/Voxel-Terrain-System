using UnityEngine;

public class JobWorld : MonoBehaviour
{
    public Material material;
    public JobWorldChunk[,] chunks = new JobWorldChunk[Data.chunkNum, Data.chunkNum];
    public Transform center;

    private int offset = Data.chunkNum * Data.chunkSize / 2;

    private void Start()
    {
        for (int x = 0; x < Data.chunkNum; x++) 
        {
            for (int z = 0; z < Data.chunkNum; z++) 
            {
                Vector3 position = new Vector3(x * Data.chunkSize, 0, z * Data.chunkSize);
                chunks[x, z] = new JobWorldChunk(material, position);
            }
        }

        for (int x = 0; x < Data.chunkNum; x++) 
        {
            for (int z = 0; z < Data.chunkNum; z++) 
            {
                chunks[x, z].ScheduleDraw();
            }
        }

        for (int x = 0; x < Data.chunkNum; x++) 
        {
            for (int z = 0; z < Data.chunkNum; z++) 
            {
                chunks[x, z].CompleteDraw();
            }
        }
    }
    
    private void Update()
    {
        RecycleChunks();

        for (int x = 0; x < Data.chunkNum; x++)
        {
            for (int z = 0; z < Data.chunkNum; z++)
            {
                chunks[x, z].ScheduleDraw();
            }
        }

        for (int x = 0; x < Data.chunkNum; x++)
        {
            for (int z = 0; z < Data.chunkNum; z++)
            {
                chunks[x, z].CompleteDraw();
            }
        }
    }

    private void RecycleChunks()
    {
        for (int x = 0; x < Data.chunkNum; x++)
        {
            for (int z = 0; z < Data.chunkNum; z++)
            {
                // x
                if (center.position.x + offset < chunks[x, z].gameObject.transform.position.x)
                {
                    chunks[x, z].gameObject.transform.position -= new Vector3(Data.chunkNum * Data.chunkSize, 0, 0);
                    chunks[x, z].needsDrawn = true;
                }
                if (center.position.x - offset > chunks[x, z].gameObject.transform.position.x)
                {
                    chunks[x, z].gameObject.transform.position += new Vector3(Data.chunkNum * Data.chunkSize, 0, 0);
                    chunks[x, z].needsDrawn = true;
                }

                // z
                if (center.position.z + offset < chunks[x, z].gameObject.transform.position.z)
                {
                    chunks[x, z].gameObject.transform.position -= new Vector3(0, 0, Data.chunkNum * Data.chunkSize);
                    chunks[x, z].needsDrawn = true;
                }
                if (center.position.z - offset > chunks[x, z].gameObject.transform.position.z)
                {
                    chunks[x, z].gameObject.transform.position += new Vector3(0, 0, Data.chunkNum * Data.chunkSize);
                    chunks[x, z].needsDrawn = true;
                }
            }
        }
    }
}
