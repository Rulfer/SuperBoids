using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class Boid : MonoBehaviour
{
    Vector3 currentPosition;
    Transform cachedTransform;

    private BoidSettings _settings;

    private void Start()
    {
        cachedTransform = this.transform;
    }

    public void Initialize(BoidSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Identify direction of boid, based on potential obstacles and nearby boids.
    /// </summary>
    public void Compute()
    {

    }

    private bool isHeadingForCollision()
    {
        RaycastHit hit;

        if(Physics.SphereCast(currentPosition, _settings.CastRadius, Vector3.forward, out hit, _settings.CastDistance, _settings.ObstacleMask))
        {
            return true;
        }
        return false;
    }
}
