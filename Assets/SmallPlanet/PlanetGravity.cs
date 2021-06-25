using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    public Transform planet;
    public Transform playerTransform;
    public Rigidbody playerRigidbody;

    public float gravity = 9.8f;
    public float planetGravityAffectRadius = 100;

    public float planetOrientationAffectRadius = 150;
    public AnimationCurve planetOrientationAffectorCurve;

    void FixedUpdate()
    {
        float distancePlanetCenter = Vector3.Distance(planet.position, playerTransform.position);

        float curvePos = distancePlanetCenter / planetOrientationAffectRadius;
        float currentAffectAmount = planetOrientationAffectorCurve.Evaluate(curvePos);

        // get the current up direction from the planet
        Vector3 upDirection = playerTransform.position - planet.position;

        // set the players up vector to be the up direction of the planet
        playerTransform.up = upDirection;

        playerTransform.up = (playerTransform.up + (upDirection * currentAffectAmount)) / 2;


        // make the magnitude of the vector 1 by normalizing it
        // we have to normalie it so that the force we add to the player for gravity is 1
        upDirection.Normalize();

        // add a force in the negative up direction
        // because the negative up direction is down
        // the neagtive up direction has a magnitude of 1 so we can multiply it by a gravity number
        playerRigidbody.AddForce(-upDirection * gravity);
    }
}
