using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimplePlanetGravity : MonoBehaviour
{
    public Transform planet;
    public float gravityForce = 9.8f;

    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // up direction from the planet center
        Vector3 upDirection = transform.position - planet.position;

        // make the magitude 1
        upDirection.Normalize();

        // add planet gravity force
        playerRigidbody.AddForce(-upDirection * gravityForce);

        // set the upDirection of the player
        transform.up = upDirection;
    }
}
