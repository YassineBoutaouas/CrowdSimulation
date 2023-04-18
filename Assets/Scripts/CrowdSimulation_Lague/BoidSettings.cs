using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Boids_Lague
{
    [CreateAssetMenu]
    public class BoidSettings : ScriptableObject
    {
        public float MinSpeed = 2;
        public float MaxSpeed = 5;
        public float PerceptionRadius = 2.5f;
        public float AvoidanceRadius = 1;
        public float MaxSteerForce = 3;

        public float AlignWeight = 1;
        public float CohesionWeight = 1;
        public float SeperateWeight = 1;

        public float TargetWeight = 1;

        public LayerMask ObstacleMask;
        public float BoundsRadius = 0.27f;
        public float AvoidCollisionWeight = 10;
        public float CollisionAvoidanceDistance = 5;
    }
}
