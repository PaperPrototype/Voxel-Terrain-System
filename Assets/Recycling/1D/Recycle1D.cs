using UnityEngine;

public class Recycle1D : MonoBehaviour
{
    public const int numCubes = 10;
    public const int offset = numCubes / 2;
    public GameObject[] cubes = new GameObject[numCubes];
    public Transform center;

    void Start()
    {
        for (int i = 0; i < numCubes; i++)
        {
            cubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubes[i].GetComponent<Transform>().position = new Vector3(i - offset, 0, 0);
        }
    }

    void Update()
    {
        for (int i = 0; i < numCubes; i++)
        {
            if (GetRoundedPos().x + offset < cubes[i].transform.position.x)
            {
                cubes[i].transform.position -= new Vector3(numCubes, 0, 0);
            }
            if (GetRoundedPos().x - offset > cubes[i].transform.position.x)
            {
                cubes[i].transform.position += new Vector3(numCubes, 0, 0);
            }
        }
    }

    private Vector3 GetRoundedPos()
    {
        return new Vector3(Mathf.Round(center.position.x), 0, 0);
    }
}
