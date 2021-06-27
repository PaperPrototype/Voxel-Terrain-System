using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    public Transform planet;
    public Transform playerTransform;
    public Rigidbody playerRigidbody;

    public float gravity = 9.8f;
    public float gravityAffectRadius = 100;
    public AnimationCurve planetGravity;

    public float orientationAffectRadius = 100;
    public AnimationCurve planetOrientation;

    public bool debug = false;
    public float debugGravityAffect;
    public float debugOrientationAffect;

    void FixedUpdate()
    {
        float distance = Vector3.Distance(planet.position, playerTransform.position);

        // get the current up direction from the planet
        Vector3 upDirection = playerTransform.position - planet.position;

        // make the magnitude of the vector 1 by normalizing it
        // we have to normalie it so that the force we add is 1, and then we can multiply it by a gravity amount
        // also normailze it so that the Lerp function works
        upDirection.Normalize();

        // slowly interplate the players up direction to be the same as the planets
        playerTransform.up = Vector3.Lerp(playerTransform.up, upDirection, CurrentOrientationAffect(distance));

        // add a force in the negative up direction
        // because the negative up direction is down
        // the neagtive up direction has a magnitude of 1 so we can multiply it by a gravity number
        playerRigidbody.AddForce(CurrentGravityAffect(distance) * gravity * -upDirection);

        if (debug == true)
        {
            debugGravityAffect = CurrentGravityAffect(distance);
            debugOrientationAffect = CurrentOrientationAffect(distance);
        }
    }

    /// <summary>
    /// Calculates the amount of orientation affect to give the player
    /// </summary>
    /// <param name="distance">the distance fromt eh center of the planet to this object</param>
    /// <returns>returns between zero and 1</returns>
    private float CurrentOrientationAffect(float distance)
    {
        // get the posiiotn on the curve
        float curvePos = distance / orientationAffectRadius;

        // clamp to 1
        if (curvePos > 1)
        {
            curvePos = 1;
        }

        // get the affect value from the curve
        float currentAffectAmount = planetOrientation.Evaluate(curvePos);

        // prevent number from becoming negative
        if (currentAffectAmount < 0)
        {
            currentAffectAmount = 0;
        }

        return currentAffectAmount;
    }

    private float CurrentGravityAffect(float distance)
    {

        // where on the animation curve to evaluate
        float curvePos = distance / gravityAffectRadius;

        // clamp to 1
        if (curvePos > 1)
        {
            curvePos = 1;
        }

        // get value from the curve
        float currentAffectAmount = planetGravity.Evaluate(curvePos);

        // prevent number from becoming negative
        if (currentAffectAmount < 0)
        {
            currentAffectAmount = 0;
        }

        return currentAffectAmount;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(planet.position, gravityAffectRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(planet.position, orientationAffectRadius);
    }
}
