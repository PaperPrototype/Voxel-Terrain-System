using UnityEngine;

public class JobWorld1 : MonoBehaviour
{
    public Material material;
    public JobWorldChunk1[,,] chunks = new JobWorldChunk1[DataDefs.chunkNum, DataDefs.chunkNum, DataDefs.chunkNum];
    public Transform center;
    public float planetRadius = 100;

    [Min(1)]
    public float noiseAmplitude = 10;

    [Range(0, 0.5f)]
    public float noiseFrequency = 1.25f;

    private int offset = DataDefs.chunkNum * DataDefs.chunkSize / 2;

    private void Start()
    {
        for (int x = 0; x < DataDefs.chunkNum; x++) 
        {
            for (int y = 0; y < DataDefs.chunkNum; y++)
            {
                for (int z = 0; z < DataDefs.chunkNum; z++)
                {
                    Vector3 position = new Vector3(x * DataDefs.chunkSize, y * DataDefs.chunkSize, z * DataDefs.chunkSize);
                    chunks[x, y, z] = new JobWorldChunk1(this, material, position, planetRadius);
                }
            }
        }

        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int y = 0; y < DataDefs.chunkNum; y++)
            {
                for (int z = 0; z < DataDefs.chunkNum; z++)
                {
                    chunks[x, y, z].ScheduleDraw();
                }
            }
        }

        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int y = 0; y < DataDefs.chunkNum; y++)
            {
                for (int z = 0; z < DataDefs.chunkNum; z++)
                {
                    chunks[x, y, z].CompleteDraw();
                }
            }
        }
    }
    
    private void Update()
    {
        RecycleChunks();

        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int y = 0; y < DataDefs.chunkNum; y++)
            {
                for (int z = 0; z < DataDefs.chunkNum; z++)
                {
                    chunks[x, y, z].ScheduleDraw();
                }
            }
        }

        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int y = 0; y < DataDefs.chunkNum; y++)
            {
                for (int z = 0; z < DataDefs.chunkNum; z++)
                {
                    chunks[x, y, z].CompleteDraw();
                }
            }
        }
    }

    private void RecycleChunks()
    {
        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int y = 0; y < DataDefs.chunkNum; y++)
            {
                for (int z = 0; z < DataDefs.chunkNum; z++)
                {
                    // x
                    if (center.position.x + offset < chunks[x, y, z].gameObject.transform.position.x)
                    {
                        chunks[x, y, z].gameObject.transform.position -= new Vector3(DataDefs.chunkNum * DataDefs.chunkSize, 0, 0);
                        chunks[x, y, z].needsDrawn = true;
                    }
                    if (center.position.x - offset > chunks[x, y, z].gameObject.transform.position.x)
                    {
                        chunks[x, y, z].gameObject.transform.position += new Vector3(DataDefs.chunkNum * DataDefs.chunkSize, 0, 0);
                        chunks[x, y, z].needsDrawn = true;
                    }

                    // y
                    if (center.position.y + offset < chunks[x, y, z].gameObject.transform.position.y)
                    {
                        chunks[x, y, z].gameObject.transform.position -= new Vector3(0, DataDefs.chunkNum * DataDefs.chunkSize, 0);
                        chunks[x, y, z].needsDrawn = true;
                    }
                    if (center.position.y - offset > chunks[x, y, z].gameObject.transform.position.y)
                    {
                        chunks[x, y, z].gameObject.transform.position += new Vector3(0, DataDefs.chunkNum * DataDefs.chunkSize, 0);
                        chunks[x, y, z].needsDrawn = true;
                    }

                    // z
                    if (center.position.z + offset < chunks[x, y, z].gameObject.transform.position.z)
                    {
                        chunks[x, y, z].gameObject.transform.position -= new Vector3(0, 0, DataDefs.chunkNum * DataDefs.chunkSize);
                        chunks[x, y, z].needsDrawn = true;
                    }
                    if (center.position.z - offset > chunks[x, y, z].gameObject.transform.position.z)
                    {
                        chunks[x, y, z].gameObject.transform.position += new Vector3(0, 0, DataDefs.chunkNum * DataDefs.chunkSize);
                        chunks[x, y, z].needsDrawn = true;
                    }
                }
            }
        }
    }
}
