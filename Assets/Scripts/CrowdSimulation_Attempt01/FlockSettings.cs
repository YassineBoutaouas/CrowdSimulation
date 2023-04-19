using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdSimulation
{
    [CreateAssetMenu(menuName = "Crowd Simulation/Settings")]
    public class FlockSettings : ScriptableObject
    {
        public float MinSpeed = 2;
        public float MaxSpeed = 5;
        public float PerceptionRadius = 2.5f;
        public float AvoidanceRadius = 1;
        public float MaxSteerForce = 3;

        [Header("Weights"), Space()]
        public float AlignWeight = 1;
        public float CohesionWeight = 1;
        public float SeperationWeight = 1;
        public float TargetWeight = 1;

        public float AvoidCollisionWeight = 10;

        [Header("Collisions"), Space()]
        public LayerMask ObstacleLayer;
        public float BoundsRadius = 0.27f;
        public float CollisionAvoidanceDistance = 5;
    }
}  