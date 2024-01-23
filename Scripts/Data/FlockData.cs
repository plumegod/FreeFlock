using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/FlockData", fileName = "FlockData")]
public class FlockData : ScriptableObject
{
    [Header("角度")][SerializeField] private float fOVAnagle;
    [SerializeField] private float smoothDamp;
    [Header("障碍躲避层")][SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Vector3[] obstacleDirections;
    

    public float FOVAngle => fOVAnagle;
    public float SmoothDamp => smoothDamp;
    public LayerMask ObstacleLayer => obstacleLayer;
    public Vector3[] ObstacleDirections => obstacleDirections;
}
