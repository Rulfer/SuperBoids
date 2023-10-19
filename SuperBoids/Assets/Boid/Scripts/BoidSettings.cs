using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoidSettings", menuName = "Boids/Settings")]

/// <summary>
/// Inspired by Sebastian Lague
/// </summary>
public class BoidSettings : ScriptableObject
{
    [Header("Speed")]
    public float minSpeed = 1f;
    public float maxSpeed = 5f;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1;
    public  float maxSteerForce = 3f;

    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;

    public float targetWeight = 1;
    
    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float collitionRadius = .27f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDst = 5;
}
