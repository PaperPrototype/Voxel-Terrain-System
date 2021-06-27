using UnityEngine;

public class SimplePlanet : MonoBehaviour
{
    public Material material;
    public SimplePlanetChunk[,,] chunks = new SimplePlanetChunk[DataDefs.chunkNum, DataDefs.chunkNum, DataDefs.chunkNum];
    public Transform player;
    public float planetRadius = 100;

    [Range(0, 0.5f)]
    public float frequency = 0.02f;

    public float amplitude = 10;

    private int offset = (DataDefs.chunkNum * DataDefs.chunkSize) / 2;

    private void Start()
    {
        for (int x = 0; x < DataDefs.chunkNum; x++)
        {
            for (int y = 0; y < DataDefs.chunkNum; y++)
            {
                for (int z = 0; z < DataDefs.chunkNum; z++)
                {
                    Vector3 position = new Vector3(x * DataDefs.chunkSize, y * DataDefs.chunkSize, z * DataDefs.chunkSize);
                    chunks[x, y, z] = new SimplePlanetChunk(this, material, position);
                }
            }
        }

        RecycleChunks();
    }

    private void Update()
    {
        RecycleChunks();

        ScheduleChunks();
        CompleteChunks();
    }

    private void ScheduleChunks()
    {
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
    }

    private void CompleteChunks()
    {
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
                    if (player.position.x + offset < chunks[x, y, z].gameObject.transform.position.x)
                    {
                        chunks[x, y, z].gameObject.transform.position -= new Vector3(DataDefs.chunkNum * DataDefs.chunkSize, 0, 0);
                        chunks[x, y, z].needsDrawn = true;
                    }
                    if (player.position.x - offset > chunks[x, y, z].gameObject.transform.position.x)
                    {
                        chunks[x, y, z].gameObject.transform.position += new Vector3(DataDefs.chunkNum * DataDefs.chunkSize, 0, 0);
                        chunks[x, y, z].needsDrawn = true;
                    }

                    // y
                    if (player.position.y + offset < chunks[x, y, z].gameObject.transform.position.y)
                    {
                        chunks[x, y, z].gameObject.transform.position -= new Vector3(0, DataDefs.chunkNum * DataDefs.chunkSize, 0);
                        chunks[x, y, z].needsDrawn = true;
                    }
                    if (player.position.y - offset > chunks[x, y, z].gameObject.transform.position.y)
                    {
                        chunks[x, y, z].gameObject.transform.position += new Vector3(0, DataDefs.chunkNum * DataDefs.chunkSize, 0);
                        chunks[x, y, z].needsDrawn = true;
                    }

                    // z
                    if (player.position.z + offset < chunks[x, y, z].gameObject.transform.position.z)
                    {
                        chunks[x, y, z].gameObject.transform.position -= new Vector3(0, 0, DataDefs.chunkNum * DataDefs.chunkSize);
                        chunks[x, y, z].needsDrawn = true;
                    }
                    if (player.position.z - offset > chunks[x, y, z].gameObject.transform.position.z)
                    {
                        chunks[x, y, z].gameObject.transform.position += new Vector3(0, 0, DataDefs.chunkNum * DataDefs.chunkSize);
                        chunks[x, y, z].needsDrawn = true;
                    }
                }
            }
        }
    }
}
