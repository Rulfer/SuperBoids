using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class Boid : MonoBehaviour
{
    //Vector3 _currentPosition;
    Transform _cachedTransform;
    public static Transform Target { get; set; } = null;

    private Vector3 _velocity;

    private BoidSettings _settings;

    public Vector3 Position;
    public Vector3 Forward;

    // To update:
    [HideInInspector]
    public Vector3 avgFlockHeading;
    [HideInInspector]
    public Vector3 avgAvoidanceHeading;
    [HideInInspector]
    public Vector3 centreOfFlockmates;
    [HideInInspector]
    public int numPerceivedFlockmates;

    [Header("Editor Settings")]
    public static bool AllowMoving = true;
    public bool DebugGizmo = false;

    public void Initialize(BoidSettings settings)
    {
        _settings = settings;
        _cachedTransform = this.transform;

        Position = _cachedTransform.position;
        Forward = _cachedTransform.forward;

        float startSpeed = (_settings.minSpeed + _settings.maxSpeed) / 2;
        _velocity = _cachedTransform.forward * startSpeed;
    }

    /// <summary>
    /// Identify direction of boid, based on potential obstacles and nearby boids.
    /// </summary>
    public void Compute()
    {
        if (!AllowMoving)
            return;

        Vector3 acceleration = Vector3.zero;

        if (Target != null)
        {
            Vector3 offsetToTarget = (Target.position - Position);
            acceleration = SteerTowards(offsetToTarget) * _settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0)
        {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - Position);

            var alignmentForce = SteerTowards(avgFlockHeading) * _settings.alignWeight;
            var cohesionForce = SteerTowards(offsetToFlockmatesCentre) * _settings.cohesionWeight;
            var seperationForce = SteerTowards(avgAvoidanceHeading) * _settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }

        if (isHeadingForCollision(out Vector3 avoidanceDirection))
        {
            // We are about to hit something
            Vector3 collisionAvoidForce = SteerTowards(avoidanceDirection) * _settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        _velocity += acceleration * Time.deltaTime;
        float speed = _velocity.magnitude;
        Vector3 dir = _velocity / speed;
        speed = Mathf.Clamp(speed, _settings.minSpeed, _settings.maxSteerForce);
        _velocity = dir * speed;

        _cachedTransform.position += _velocity * Time.deltaTime;
        _cachedTransform.forward = dir;
        Position = _cachedTransform.position;
        Forward = dir;
    }

    /// <summary>
    /// Detect if this object is near to collide with any other.
    /// </summary>
    private bool isHeadingForCollision(out Vector3 avoidanceDirection)
    {
        RaycastHit hit;
        avoidanceDirection = _cachedTransform.forward;

        if (Physics.SphereCast(Position, _settings.collitionRadius, Forward, out hit, _settings.collisionAvoidDst, _settings.obstacleMask))
        {
            // Collision detected
            avoidanceDirection = ObstacleRays();
            return true;
        }
        return false;
    }

    Vector3 ObstacleRays()
    {
        Vector3[] rayDirections = BoidHelper.directions;

        for (int i = 0; i < rayDirections.Length; i++)
        {
            Vector3 dir = _cachedTransform.TransformDirection(rayDirections[i]);
            if(DebugGizmo)
            {
                Debug.DrawRay(Position, dir);
            }


            Ray ray = new Ray(Position, dir);
            if (!Physics.SphereCast(ray, _settings.collitionRadius, _settings.collisionAvoidDst, _settings.obstacleMask))
            {
                return dir;
            }
        }

        return Forward;
    }

    Vector3 SteerTowards(Vector3 vector)
    {
        Vector3 v = vector.normalized * _settings.maxSpeed - _velocity;
        return Vector3.ClampMagnitude(v, _settings.maxSteerForce);
    }

    private void OnDrawGizmos()
    {
        if (_cachedTransform == null || !DebugGizmo)
            return;

        RaycastHit hit;
        Vector3 spherePosition = Position + Forward * _settings.collisionAvoidDst;

        if (Physics.SphereCast(Position, _settings.collitionRadius, Forward, out hit, _settings.collisionAvoidDst, _settings.obstacleMask))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spherePosition, _settings.collitionRadius);
            Debug.DrawLine(_cachedTransform.position, spherePosition, Color.red);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spherePosition, _settings.collitionRadius);
            Debug.DrawLine(_cachedTransform.position, spherePosition, Color.green);
        }
    }
}

public static class BoidHelper
{

    const int numViewDirections = 300;
    public static readonly Vector3[] directions;

    static BoidHelper()
    {
        Debug.Log(" BoidHelper()");
        directions = new Vector3[BoidHelper.numViewDirections];

        float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
        float angleIncrement = Mathf.PI * 2 * goldenRatio;

        for (int i = 0; i < numViewDirections; i++)
        {
            float t = (float)i / numViewDirections;
            float inclination = Mathf.Acos(1 - 2 * t);
            float azimuth = angleIncrement * i;

            float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
            float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
            float z = Mathf.Cos(inclination);
            directions[i] = new Vector3(x, y, z);
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(Boid))]
public class BoidEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        // Check if a button is pressed
        if (GUILayout.Button("Toggle Boid Movement on/off"))
        {
            Boid.AllowMoving = !Boid.AllowMoving;
        }

        if(Application.isPlaying && GUILayout.Button("Render Gizmo"))
        {
            Boid boid = (Boid)target;
            boid.DebugGizmo = !boid.DebugGizmo;
        }
    }

}
#endif