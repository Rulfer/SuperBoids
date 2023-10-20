using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    const int threadGroupSize = 1024;

    [SerializeField] private Boid _boidPrefab;
    [SerializeField] private BoidSettings _boidSettings;
    [SerializeField] private Transform _boidTarget;

    [Header("Settings")]
    [Tooltip("Number of boids to spawn")]
    [SerializeField] private int _spawnCount = 100;
    [SerializeField] private float _spawnRadius = 1.0f;

    [Header("GPU computing")]
    [SerializeField] private bool _attemptToComputeIndividualBoid = true;

    /// <summary>
    /// Reference to all alive boids.
    /// </summary>
    private List<Boid> _boids = new List<Boid>();

    public ComputeShader _computeFlock;

    private void Awake()
    {
        _boidSettings.Initialize(inheritedMonoBehaviour: this);
        SpawnBoids();
    }

    // Update is called once per frame
    void Update()
    {
        if (_boids == null || _boids.Count <= 0)
            return;

        CalculateFlock();
        CalculateIndividualBoid();

        //if (_boids.Count <= 0)
        //    return;

        //// Thread, or execute on compute shader
        //foreach(Boid boid in _boids)
        //{
        //    boid.Compute();
        //}
    }

    private void CalculateFlock()
    {
        int numBoids = _boids.Count;
        var boidData = new BoidData[numBoids];

        for (int i = 0; i < _boids.Count; i++)
        {
            boidData[i].position = _boids[i].Position;
            boidData[i].direction = _boids[i].Forward;
            boidData[i].velocity = _boids[i].Velocity;
            boidData[i].generatedAcceleration = Vector3.zero;
            boidData[i].flockHeading = Vector3.zero;
            boidData[i].flockCentre = Vector3.zero;
            boidData[i].avoidanceHeading = Vector3.zero;

            boidData[i].numFlockmates= 0;
            boidData[i].status = -1;
        }

        var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
        boidBuffer.SetData(boidData);

        _computeFlock.SetBuffer(0, "boids", boidBuffer);
        _computeFlock.SetInt("numBoids", _boids.Count);
        _computeFlock.SetFloat("viewRadius", _boidSettings.perceptionRadius);
        _computeFlock.SetFloat("avoidRadius", _boidSettings.avoidanceRadius);

        // Target
        _computeFlock.SetBool("hasTarget", Boid.Target != null);
        if (Boid.Target != null)
        {
            _computeFlock.SetVector("targetPosition", Boid.Target.position);
        }

        // Individual movement
        _computeFlock.SetFloat("maxSpeed", _boidSettings.maxSpeed);
        _computeFlock.SetFloat("maxSteerForce", _boidSettings.maxSteerForce);
        _computeFlock.SetFloat("targetWeight", _boidSettings.targetWeight);
        _computeFlock.SetFloat("alignWeight", _boidSettings.alignWeight);
        _computeFlock.SetFloat("cohesionWeight", _boidSettings.cohesionWeight);
        _computeFlock.SetFloat("seperateWeight", _boidSettings.seperateWeight);


        int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
        _computeFlock.Dispatch(0, threadGroups, 1, 1);

        boidBuffer.GetData(boidData);

        if (!Boid.AllowMoving)
        {
            boidBuffer.Release();
            return;
        }

        for (int i = 0; i < _boids.Count; i++)
        {
            _boids[i].avgFlockHeading = boidData[i].flockHeading;
            _boids[i].centreOfFlockmates = boidData[i].flockCentre;
            _boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
            _boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

            //_boids[i].Velocity += boidData[i].generatedAcceleration * Time.deltaTime;

            if (_attemptToComputeIndividualBoid)
            {
                Vector3 acceleration = boidData[i].generatedAcceleration;
                _boids[i].ComputeWithShaderHelp(acceleration);
            }
            else
            {

                _boids[i].Compute();
            }

            //if(Boid.AllowMoving && !_attemptToComputeIndividualBoid)
            //_boids[i].Compute();
        }

        boidBuffer.Release();

        //if(_attemptToComputeIndividualBoid)
        //{
        //    for(int i = 0; i < _boids.Count; i++)
        //    {
        //        _boids[i].ComputeWithShaderHelp();
        //    }
        //}
    }

    private void CalculateIndividualBoid()
    {

    }

    public void SetBoidTarget(Transform target)
    {
        _boidSettings.OnFollowTarget();
        Boid.Target = target;
    }

    public void ClearBoidTarget()
    {
        _boidSettings.OnIdleAround();
        Boid.Target = null;
    }

    public void SpawnBoids()
    {
        DeleteExistingBoids();

        for (int i = 0; i < _spawnCount; i++)
        {
            Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * _spawnRadius;

            Boid newBoid = Instantiate(_boidPrefab, randomPosition, Quaternion.identity);
            newBoid.Initialize(_boidSettings);
            _boids.Add(newBoid);
        }
    }

    public void DeleteExistingBoids()
    {
        if (_boids.Count <= 0)
            return;

        foreach (Boid boid in _boids)
        {
            Destroy(boid.gameObject);
        }
        _boids.Clear();
    }

    public struct BoidData
    {
        public float3 position;
        public float3 direction;
        public float3 velocity;
        public float3 generatedAcceleration;

        public float3 flockHeading;
        public float3 flockCentre;
        public float3 avoidanceHeading;
        public int numFlockmates;
        public int status;

        public static int Size
        {
            get
            {
                // We have Vector 3, which is sizeof(float) * 3, and we have 6 vectors, so * 6.
                // The Size is the total size of all variable types and the number of them in this struct.
                return (sizeof(float) * 3 * 7) + (sizeof(int) * 2);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoidManager))]
public class BoidManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BoidManager boidManager = (BoidManager)target;

        // Check if a button is pressed
        if (GUILayout.Button("Restart Boids"))
        {
            if (!Application.isPlaying)
                return;
            boidManager.SpawnBoids();
        }
    }
}
#endif
