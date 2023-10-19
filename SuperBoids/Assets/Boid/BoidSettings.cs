using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoidSettings", menuName = "Boids/Settings")]
public class BoidSettings : ScriptableObject
{
    [Header("Raycast Settings")]
    [SerializeField] private float _castRadius = 180f;
    [SerializeField] private float _castDistance = 0.25f;
    [SerializeField] private LayerMask _obstacleMask;

    public float CastRadius { get => _castRadius; }
    public float CastDistance { get => _castDistance; }
    public LayerMask ObstacleMask { get => _obstacleMask; }


    public float speed;

}
