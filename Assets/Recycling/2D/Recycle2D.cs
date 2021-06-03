using UnityEngine;

public class Recycle2D : MonoBehaviour
{
    const int numCubes = 10;
    public const int offset = numCubes / 2;
    public GameObject[,] cubes = new GameObject[numCubes, numCubes];
    public Transform center;

    private void Start()
    {
        int i = 0;
        for (int x = (int)GetRoundedPos().x; x < numCubes + (int)GetRoundedPos().x; x++)
        {
            for (int z = (int)GetRoundedPos().z; z < numCubes + (int)GetRoundedPos().z; z++)
            {
                cubes[x, z] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubes[x, z].GetComponent<Transform>().position = new Vector3(x, 0, z);
                cubes[x, z].gameObject.name = "(" + x + " " + z + ") " + "2D index conversion: " + ((x * numCubes) + z) + " actual index: " + i;
                i++;
            }
        }
    }

    private void Update()
    {
        for (int x = 0; x < numCubes; x++)
        {
            for (int z = 0; z < numCubes; z++)
            {
                // x
                if (GetRoundedPos().x + offset < cubes[x, z].transform.position.x)
                {
                    cubes[x, z].transform.position += new Vector3(-numCubes, 0, 0);
                }
                else
                if (GetRoundedPos().x - offset > cubes[x, z].transform.position.x)
                {
                    cubes[x, z].transform.position += new Vector3(numCubes, 0, 0);
                }

                // z
                if (GetRoundedPos().z + offset < cubes[x, z].transform.position.z)
                {
                    cubes[x, z].transform.position += new Vector3(0, 0, -numCubes);
                }
                else
                if (GetRoundedPos().z - offset > cubes[x, z].transform.position.z)
                {
                    cubes[x, z].transform.position += new Vector3(0, 0, numCubes);
                }
            }
        }
    }

    private Vector3 GetRoundedPos()
    {
        return new Vector3(Mathf.Round(center.position.x), 0, Mathf.Round(center.position.z));
    }
}
