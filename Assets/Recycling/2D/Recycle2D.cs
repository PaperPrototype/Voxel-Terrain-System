using UnityEngine;

public class Recycle2D : MonoBehaviour
{
    const int numCubes = 10;
    public const int offset = numCubes / 2;
    public GameObject[,] cubes = new GameObject[numCubes, numCubes];
    public Transform center;

    private void Start()
    {
        for (int x = 0; x < numCubes; x++)
        {
            for (int z = 0; z < numCubes; z++)
            {
                cubes[x, z] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubes[x, z].GetComponent<Transform>().position = new Vector3(x - offset, 0, z - offset);
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
                if (center.position.x + offset < cubes[x, z].transform.position.x)
                {
                    cubes[x, z].transform.position += new Vector3(-numCubes, 0, 0);
                }
                else
                if (center.position.x - offset > cubes[x, z].transform.position.x)
                {
                    cubes[x, z].transform.position += new Vector3(numCubes, 0, 0);
                }

                // z
                if (center.position.z + offset < cubes[x, z].transform.position.z)
                {
                    cubes[x, z].transform.position += new Vector3(0, 0, -numCubes);
                }
                else
                if (center.position.z - offset > cubes[x, z].transform.position.z)
                {
                    cubes[x, z].transform.position += new Vector3(0, 0, numCubes);
                }
            }
        }
    }
}
