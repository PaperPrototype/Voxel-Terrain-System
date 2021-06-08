using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PlayerVoxelEdit : MonoBehaviour
{
    public Camera cam;
    public EditableChunk chunk;
    public GameObject prefab;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 desiredPoint = hit.point - (hit.normal / 2);



                int3 gridPosition = new int3(Mathf.RoundToInt(desiredPoint.x), Mathf.RoundToInt(desiredPoint.y), Mathf.RoundToInt(desiredPoint.z));

                chunk.data[gridPosition.x, gridPosition.y, gridPosition.z] = 0;
                chunk.DrawChunk();
            }
        }
    }
}
