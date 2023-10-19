using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [SerializeField] private Boid _boidPrefab;
    [SerializeField] private BoidSettings _boidSettings;

    [Header("Settings")]
    [Tooltip("Number of boids to spawn")]
    [SerializeField] private int _spawnCount = 100;

    /// <summary>
    /// Reference to all alive boids.
    /// </summary>
    private List<Boid> _boids = new List<Boid>();



    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnBoids()
    {
        DeleteExistingBoids();

        for(int i = 0; i < _spawnCount; i++)
        {
            Boid newBoid = Instantiate(_boidPrefab);
            newBoid.Initialize(_boidSettings);
            _boids.Add(newBoid);
        }
    }

    public void DeleteExistingBoids()
    {
        if (_boids.Count <= 0)
            return;

        foreach(Boid boid in _boids)
        {
            Destroy(boid.gameObject);
        }
        _boids.Clear();
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
        if (GUILayout.Button("Spawn Boids"))
        {
            if (!Application.isPlaying)
                return;
            boidManager.SpawnBoids();
        }
    }
}
#endif
