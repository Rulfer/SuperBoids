using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Tooltip("Simple target the Boids try to move towards")]
public class BoidTarget : MonoBehaviour
{
    private BoidManager _boidManager;

    [Header("Settings")]
    [SerializeField] public TargetMode mode = TargetMode.pingPong;

    [Header("Array")]
    [SerializeField] private float _moveTowardsSpeed = 1f;
    [SerializeField] Transform[] _targets;
    [HideInInspector] public bool DoFollowTarget = false;
    [SerializeField] public int _targetIndex = 0;
    [Tooltip("How near this object has to be the other transform for it to be registered as 'I hit it, so change target'.")]
    [SerializeField] public float _positionThreshold = 1f;
    

    [Header("Ping Pong")]
    [SerializeField] Transform _pingPongTargetA;
    [SerializeField] Transform _pingPongTargetB;
    [SerializeField] float _pingPongSpeed = 2f;
    [HideInInspector] public bool DoPingPong = false;

    public enum TargetMode
    {
        pingPong,
        array,
        idle
    }

    private void Start()
    {
        _boidManager = GameObject.FindObjectOfType<BoidManager>();
    }

    private void Update()
    {
        switch(mode)
        {
            case TargetMode.pingPong:
                float pingPong = Mathf.PingPong(Time.time * _pingPongSpeed, 1);
                this.transform.position = Vector3.Lerp(_pingPongTargetA.position, _pingPongTargetB.position, pingPong);
                return;

            case TargetMode.array:
                if (Vector3.Distance(transform.position, _targets[_targetIndex].position) < 1f)
                {
                    Debug.Log(this + " distance is " + Vector3.Distance(transform.position, _targets[_targetIndex].position));
                    _targetIndex = (_targetIndex + 1) % _targets.Length;
                }

                this.transform.position = Vector3.MoveTowards(this.transform.position, _targets[_targetIndex].position, _moveTowardsSpeed * Time.deltaTime);
                return;
        }
    }

    public void SetAsTarget()
    {
        _boidManager.SetBoidTarget(this.transform);
    }

    public void RemoveAsTarget()
    {
        _boidManager.ClearBoidTarget();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BoidTarget))]
public class BoidTargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        //DrawDefaultInspector();

        BoidTarget boidTarget = (BoidTarget)target;

        EditorGUILayout.PropertyField(serializedObject.FindProperty("mode"));


        switch (boidTarget.mode)
        {
            case BoidTarget.TargetMode.pingPong:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_pingPongTargetA"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_pingPongTargetB"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_pingPongSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_positionThreshold"));
                break;

            case BoidTarget.TargetMode.array:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_moveTowardsSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_targets"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetIndex"));
                break;

        }

        // Apply modifications.
        serializedObject.ApplyModifiedProperties();

        if (!Application.isPlaying)
            return;

        if (GUILayout.Button("Toggle PingPong"))
        {
            boidTarget.mode = BoidTarget.TargetMode.pingPong;
        }
        if (GUILayout.Button("Toggle Follow Array"))
        {
            boidTarget.mode = BoidTarget.TargetMode.array;
        }
        if (GUILayout.Button("Stand still"))
        {
            boidTarget.mode = BoidTarget.TargetMode.idle;
        }

        if (GUILayout.Button("Set As Boid Target"))
        {
            boidTarget.SetAsTarget();
        }
        if (GUILayout.Button("Clear Boid Target"))
        {
            boidTarget.RemoveAsTarget();
        }
    }
}
#endif
