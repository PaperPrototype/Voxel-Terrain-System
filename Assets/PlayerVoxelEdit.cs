using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVoxelEdit : MonoBehaviour
{
    public Camera cam;
    public EditableChunk chunk;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                chunk.data[(int)hit.point.x, (int)hit.point.y, (int)hit.point.z] = 0;
            }
        }
    }
}
