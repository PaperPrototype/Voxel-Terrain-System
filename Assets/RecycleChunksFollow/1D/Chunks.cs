using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunks : MonoBehaviour
{
    public const int chunkSize = 10;
    public const int offset = chunkSize / 2;
    public GameObject[] chunks = new GameObject[chunkSize];
    public Transform center;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = (int)GetRoundedPos().x; i < chunkSize + (int)GetRoundedPos().x; i++)
        {
            chunks[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunks[i].GetComponent<Transform>().position = new Vector3(i, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < chunkSize; i++)
        {
            if (GetRoundedPos().x + offset < chunks[i].transform.position.x)
            {
                chunks[i].transform.position += new Vector3(-chunkSize, 0, 0);
            }
            else
            if (GetRoundedPos().x - offset > chunks[i].transform.position.x)
            {
                chunks[i].transform.position += new Vector3(chunkSize, 0, 0);
            }
        }

    }

    private Vector3 GetRoundedPos()
    {
        return new Vector3(Mathf.Round(center.position.x), 0, 0);
    }

}
