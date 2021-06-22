using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    // movement
    public float jumpVelocity = 6;
    public float moveSpeed = 2;
    private Rigidbody m_rb;

    // rotation
    public float lookSpeed = 100;
    public Transform head;
    public Transform cam;

    private void Start()
    {
        m_rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            UpdateMovement();

            if (Input.GetMouseButton(1))
            {
                UpdateLook();
            }
        }
    }

    private void UpdateMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_rb.velocity += new Vector3(0, jumpVelocity, 0);
        }

        float forwardSpeed = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float sideSpeed = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

        transform.position += head.forward * forwardSpeed; // forward
        transform.position += head.right * sideSpeed; // sideways
    }

    private void UpdateLook()
    {
        float rotateSpeedy = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float rotateSpeedx = -Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;

        head.Rotate(Vector3.up, rotateSpeedy);
        cam.Rotate(Vector3.right, rotateSpeedx);
    }

    /*
    // movement
    public float jumpVelocity = 6;
    public float moveSpeed = 2; // meters per second
    private Rigidbody m_rb;

    // look rotation
    public float lookSpeed = 2; // radians per second
    public Transform head;
    public Transform cam; // nested under head

    // Start is called before the first frame update
    void Start()
    {
        m_rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
        if (Input.GetMouseButton(1))
        {
            UpdateLook();
        }
    }

    private void UpdateMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_rb.velocity += new Vector3(0, jumpVelocity, 0);
        }

        float forwardSpeed = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        float sideSpeed = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;

        transform.position += head.forward * forwardSpeed; // forward
        transform.position += head.right * sideSpeed; // sideways
    }

    private void UpdateLook()
    {
        float rotateSpeedy = Input.GetAxis("Mouse X") * lookSpeed * Time.deltaTime;
        float rotateSpeedx = -Input.GetAxis("Mouse Y") * lookSpeed * Time.deltaTime;

        head.Rotate(Vector3.up, rotateSpeedy);
        cam.Rotate(Vector3.right, rotateSpeedx);
    }
    */
}
