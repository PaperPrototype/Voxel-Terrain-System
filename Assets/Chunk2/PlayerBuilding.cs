using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuilding : MonoBehaviour
{
    public Camera cam;
    public Chunk2 chunk;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 desiredPoint = hit.point - (hit.normal / 2);
                
                chunk.EditChunkData(desiredPoint, 0);
            }
        }
    }
}
