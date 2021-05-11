using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LodFollow : MonoBehaviour
{
    public uint lodIncrement = 100;
    public Transform center;

    private const int worldSize = 1000;
    private const int offset = worldSize / 2;
    private GameObject[] chunks = new GameObject[worldSize];

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < worldSize; i++)
        {
            chunks[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunks[i].GetComponent<Transform>().position = new Vector3(i - offset, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < worldSize; i++)
        {
            if (GetRoundedPos().x + offset < chunks[i].transform.position.x)
            {
                chunks[i].transform.position += new Vector3(-worldSize, 0, 0);
            }
            else
            if (GetRoundedPos().x - offset > chunks[i].transform.position.x)
            {
                chunks[i].transform.position += new Vector3(worldSize, 0, 0);
            }

            int averageDistance = Mathf.RoundToInt(Vector3.Distance(GetRoundedPos(), chunks[i].transform.position));

            int lodLevel = averageDistance * averageDistance / (Data.chunkSize * Data.chunkSize) / (int)lodIncrement;

            chunks[i].transform.localScale = new Vector3(1, lodLevel, 1);
        }

    }

    private Vector3 GetRoundedPos()
    {
        return new Vector3(Mathf.Round(center.position.x), 0, 0);
    }
}
