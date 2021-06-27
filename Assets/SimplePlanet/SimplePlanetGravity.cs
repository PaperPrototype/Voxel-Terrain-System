using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimplePlanetGravity : MonoBehaviour
{
    public Transform planet;
    public float gravityForce = 9.8f;

    public float orientationAffectRadius = 100f;
    public AnimationCurve orientationAffectCurve;

    public float gravityAffectRadius = 100f;
    public AnimationCurve gravityAffectCurve;

    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = gameObject.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        // up direction from the planet center
        Vector3 planetUpDirection = transform.position - planet.position;

        // make the magitude 1
        planetUpDirection.Normalize();

        // distance from the center of the planet
        float distance = Vector3.Distance(planet.position, transform.position);

        // add planet gravity force
        playerRigidbody.AddForce(-planetUpDirection * gravityForce * CurrentGravityAffect(distance));

        // set the upDirection of the player
        transform.up = Vector3.Lerp(transform.up, planetUpDirection, CurrentOrientationAffect(distance));
    }

    private float CurrentOrientationAffect(float distance)
    {
        // get where on the curve to sample
        float curvePos = distance / orientationAffectRadius;

        // if we go outside of the radius we clamp it at 1
        if (curvePos > 1)
        {
            curvePos = 1;
        }

        float currentAffectAmount = orientationAffectCurve.Evaluate(curvePos);

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
        float currentAffectAmount = gravityAffectCurve.Evaluate(curvePos);

        // prevent number from becoming negative
        if (currentAffectAmount < 0)
        {
            currentAffectAmount = 0;
        }

        return currentAffectAmount;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(planet.position, orientationAffectRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(planet.position, gravityAffectRadius);
    }
}
