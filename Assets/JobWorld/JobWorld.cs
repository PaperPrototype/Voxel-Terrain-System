using UnityEngine;

public class JobWorld : MonoBehaviour
{
    public Material material;
    public JobWorldChunk[,] chunks = new JobWorldChunk[DataDefs.chunkNum, DataDefs.chunkNum];
    public Transform center;

    private int offset = DataDefs.chunkNum * DataDefs.chunkSize / 2;

    private void Start()
    {
        for (int x = 0; x < DataDefs.chunkNum; x++) 
        {
            for (int z = 0; z < DataDefs.chunkNum; z++) 
            {
                Vector3 position = new Vector3(x * DataDefs.chunkSize, 0, z * DataDefs.chunkSize);
                chunks[x, z] = new JobWorldChunk(material, position);
            }
        }

        for (int x = 0; x < DataDefs.chunkNum; x++) 
        {
            for (int z = 0; z < DataDefs.chunkNum; z++) 
            {
                chunks[x, z].ScheduleDraw();
            }
        }

        for (int x = 0; x < DataDefs.chunkNum; x++) 
        {
            for (int z = 0; z < DataDefs.chunkNum; z++) 
            {
                chunks[x, z].CompleteDraw();
            }
        }
    }
    
    private void Update()
    {
        RecycleChunks();

        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int z = 0; z < DataDefs.chunkNum; z++)
            {
                chunks[x, z].ScheduleDraw();
            }
        }

        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int z = 0; z < DataDefs.chunkNum; z++)
            {
                chunks[x, z].CompleteDraw();
            }
        }
    }

    private void RecycleChunks()
    {
        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int z = 0; z < DataDefs.chunkNum; z++)
            {
                // x
                if (center.position.x + offset < chunks[x, z].gameObject.transform.position.x)
                {
                    chunks[x, z].gameObject.transform.position -= new Vector3(DataDefs.chunkNum * DataDefs.chunkSize, 0, 0);
                    chunks[x, z].needsDrawn = true;
                }
                if (center.position.x - offset > chunks[x, z].gameObject.transform.position.x)
                {
                    chunks[x, z].gameObject.transform.position += new Vector3(DataDefs.chunkNum * DataDefs.chunkSize, 0, 0);
                    chunks[x, z].needsDrawn = true;
                }

                // z
                if (center.position.z + offset < chunks[x, z].gameObject.transform.position.z)
                {
                    chunks[x, z].gameObject.transform.position -= new Vector3(0, 0, DataDefs.chunkNum * DataDefs.chunkSize);
                    chunks[x, z].needsDrawn = true;
                }
                if (center.position.z - offset > chunks[x, z].gameObject.transform.position.z)
                {
                    chunks[x, z].gameObject.transform.position += new Vector3(0, 0, DataDefs.chunkNum * DataDefs.chunkSize);
                    chunks[x, z].needsDrawn = true;
                }
            }
        }
    }
}
