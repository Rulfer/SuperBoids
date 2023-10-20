using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "BoidSettings", menuName = "Boids/Settings")]

/// <summary>
/// Inspired by Sebastian Lague
/// </summary>
public class BoidSettings : ScriptableObject
{
    [Header("Speed")]
    public float minSpeed = 2f;
    public float maxSpeed = 5f;
    public float perceptionRadius = 2.5f;
    public float avoidanceRadius = 1;
    public float maxSteerForce = 3f;

    [Header("Idle speeds")]
    public float idleMinSpeed = 2f;
    public float idleMaxSpeed = 5f;
    public float idleMaxSteerForce = 3f;

    [Header("Follow Target Speeds")]
    public float followTargetMinSpeed = 5f;
    public float followTargetMaxSpeed = 10f;
    public float followTargetMaxSteerForce = 5f;

    public float alignWeight = 1;
    public float cohesionWeight = 1;
    public float seperateWeight = 1;

    [Tooltip("How hard the Boid tries to move towards the assigned target, if any.")]
    public float targetWeight = 1;

    [Header("Collisions")]
    public LayerMask obstacleMask;
    public float collitionRadius = .1f;
    public float avoidCollisionWeight = 10;
    public float collisionAvoidDst = 5;


    [Header("Animation settings")]
    [SerializeField] private float _timeInSecondsToChangeSpeed = 0.75f;
    [SerializeField] private AnimationCurve _sexyCurves;
    private Coroutine _changeSpeedCoroutine = null;
    /// <summary>
    /// We can use this to start/stop coroutines
    /// </summary>
    private MonoBehaviour _referencedBehaviour;

    enum BoidMode
    {
        idle,
        hunt
    }

    public void Initialize(MonoBehaviour inheritedMonoBehaviour)
    {
        _referencedBehaviour = inheritedMonoBehaviour;

        minSpeed = idleMinSpeed;
        maxSpeed = idleMaxSpeed;
        maxSteerForce = idleMaxSteerForce;

        //_minSpeed = minSpeed;
        //_maxSpeed = maxSpeed;
        //_perceptionRadius = perceptionRadius;
        //_avoidanceRadius = avoidanceRadius;
        //_maxSteerForce = maxSteerForce;
        //_alignWeight = alignWeight;
        //_cohesionWeight = cohesionWeight;
        //_seperateWeight = seperateWeight;
        //_targetWeight = targetWeight;
        //_collitionRadius = collitionRadius;
        //_avoidCollisionWeight = avoidCollisionWeight;
        //_collisionAvoidDst = collisionAvoidDst;
    }

    public void OnFollowTarget()
    {
        //minSpeed = followTargetMinSpeed;
        //maxSpeed = followTargetMaxSpeed;
        //maxSteerForce = followTargetMaxSteerForce;
        if (_changeSpeedCoroutine != null)
            _referencedBehaviour.StopCoroutine(_changeSpeedCoroutine);

        _changeSpeedCoroutine = _referencedBehaviour.StartCoroutine(AccelerateSpeed(BoidMode.hunt));
    }

    /// <summary>
    /// Don't follow anything. Just mindlessly fly around
    /// </summary>
    public void OnIdleAround()
    {
        //minSpeed = _minSpeed;
        //maxSpeed = _maxSpeed;
        //maxSteerForce = _maxSteerForce;

        if(_changeSpeedCoroutine != null)
            _referencedBehaviour.StopCoroutine(_changeSpeedCoroutine);

        _changeSpeedCoroutine = _referencedBehaviour.StartCoroutine(AccelerateSpeed(BoidMode.idle));
    }

    private IEnumerator AccelerateSpeed(BoidMode mode)
    {
        float targetMinSpeed = mode == BoidMode.hunt ? followTargetMinSpeed : idleMinSpeed;
        float targetMaxSpeed = mode == BoidMode.hunt ? followTargetMaxSpeed : idleMaxSpeed;
        float targetSteerForce = mode == BoidMode.hunt ? followTargetMaxSteerForce: idleMaxSteerForce;

        float timer = 0;

        while (timer < _timeInSecondsToChangeSpeed)
        {
            timer += Time.deltaTime;
            float timeInAnimation = _sexyCurves.Evaluate(timer / _timeInSecondsToChangeSpeed);
            minSpeed = Mathf.Lerp(minSpeed, targetMinSpeed, timeInAnimation);
            maxSpeed = Mathf.Lerp(maxSpeed, targetMaxSpeed, timeInAnimation);
            maxSteerForce = Mathf.Lerp(maxSteerForce, targetSteerForce, timeInAnimation);

            yield return null;
        }

        _changeSpeedCoroutine = null;
    }

    [ContextMenu("Reset Values To Default")]
    ///<summary>
    ///This resets all values to factory default (aka the values Sebastian Legue recommended).
    /// </summary>
    public void ResetValuesToDefault()
    {
        idleMinSpeed = 2f;
        idleMaxSpeed = 5f;
        perceptionRadius = 2.5f;
        avoidanceRadius = 1f;
        idleMaxSteerForce = 3f;
        alignWeight = 1f;
        cohesionWeight = 1f;
        seperateWeight = 1f;
        targetWeight = 1f;
        collitionRadius = 0.25f;
        avoidCollisionWeight = 10f;
        collisionAvoidDst = 5f;
    }

}


