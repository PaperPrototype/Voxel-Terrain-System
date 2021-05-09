using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunks2D : MonoBehaviour
{
    const int chunkSize = 10;
    public const int offset = chunkSize / 2;
    public GameObject[,] chunks = new GameObject[chunkSize, chunkSize];
    public Transform center;

    // Start is called before the first frame update
    private void Start()
    {
        int i = 0;
        for (int x = (int)GetRoundedPos().x; x < chunkSize + (int)GetRoundedPos().x; x++)
        {
            for (int z = (int)GetRoundedPos().z; z < chunkSize + (int)GetRoundedPos().z; z++)
            {
                chunks[x, z] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                chunks[x, z].GetComponent<Transform>().position = new Vector3(x, 0, z);
                chunks[x, z].gameObject.name = "(" + x + " " + z + ") " + ((x * chunkSize) + z) + " index: " + i;
                i++;
            }
        }
    }

    private void Update()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                // x
                if (GetRoundedPos().x + offset < chunks[x, z].transform.position.x)
                {
                    chunks[x, z].transform.position += new Vector3(-chunkSize, 0, 0);
                }
                else
                if (GetRoundedPos().x - offset > chunks[x, z].transform.position.x)
                {
                    chunks[x, z].transform.position += new Vector3(chunkSize, 0, 0);
                }

                // z
                if (GetRoundedPos().z + offset < chunks[x, z].transform.position.z)
                {
                    chunks[x, z].transform.position += new Vector3(0, 0, -chunkSize);
                }
                else
                if (GetRoundedPos().z - offset > chunks[x, z].transform.position.z)
                {
                    chunks[x, z].transform.position += new Vector3(0, 0, chunkSize);
                }
            }
        }
    }

    private Vector3 GetRoundedPos()
    {
        return new Vector3(Mathf.Round(center.position.x), 0, Mathf.Round(center.position.z));
    }
}
