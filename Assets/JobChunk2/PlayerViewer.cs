using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerViewer : MonoBehaviour
{
    public float moveSpeed = 3;
    // look rotation
    public float lookSpeed = 100; // radians per second
    public Transform cam; // nested under head

    private void Update()
    {
        UpdateMovement();
        if (Input.GetMouseButton(1))
        {
            UpdateLook();
        }
    }

    private void UpdateLook()
    {
        float rotateSpeedy = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float rotateSpeedx = -Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;

        transform.Rotate(Vector3.up, rotateSpeedy);
        cam.Rotate(Vector3.right, rotateSpeedx);
    }

    private void UpdateMovement()
    {
        float forwardSpeed = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float sideSpeed = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float upDown = Input.GetAxis("UpDown") * moveSpeed * Time.deltaTime;

        transform.position += cam.up * upDown; // upDown
        transform.position += cam.forward * forwardSpeed; // forward
        transform.position += cam.right * sideSpeed; // sideways
    }
}
