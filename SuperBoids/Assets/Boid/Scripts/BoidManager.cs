using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// Reference to all alive boids.
    /// </summary>
    private List<Boid> _boids = new List<Boid>();

    public ComputeShader compute;

    private void Start()
    {
        SpawnBoids();
    }

    // Update is called once per frame
    void Update()
    {
        if (_boids == null || _boids.Count <= 0)
            return;

        int numBoids = _boids.Count;
        var boidData = new BoidData[numBoids];

        for (int i = 0; i < _boids.Count; i++)
        {
            boidData[i].position = _boids[i].Position;
            boidData[i].direction = _boids[i].Forward;
        }

        var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
        boidBuffer.SetData(boidData);

        compute.SetBuffer(0, "boids", boidBuffer);
        compute.SetInt("numBoids", _boids.Count);
        compute.SetFloat("viewRadius", _boidSettings.perceptionRadius);
        compute.SetFloat("avoidRadius", _boidSettings.avoidanceRadius);

        int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
        compute.Dispatch(0, threadGroups, 1, 1);

        boidBuffer.GetData(boidData);

        for (int i = 0; i < _boids.Count; i++)
        {
            _boids[i].avgFlockHeading = boidData[i].flockHeading;
            _boids[i].centreOfFlockmates = boidData[i].flockCentre;
            _boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
            _boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

            _boids[i].Compute();
        }

        boidBuffer.Release();

        //if (_boids.Count <= 0)
        //    return;

        //// Thread, or execute on compute shader
        //foreach(Boid boid in _boids)
        //{
        //    boid.Compute();
        //}
    }

    public void SpawnBoids()
    {
        DeleteExistingBoids();

        for (int i = 0; i < _spawnCount; i++)
        {
            Vector3 randomPosition = Random.insideUnitSphere * _spawnRadius;

            Boid newBoid = Instantiate(_boidPrefab, randomPosition, Quaternion.identity);
            newBoid.Initialize(_boidSettings, _boidTarget);
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
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int);
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
