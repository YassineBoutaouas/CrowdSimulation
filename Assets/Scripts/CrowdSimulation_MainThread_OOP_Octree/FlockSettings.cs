using UnityEngine;

namespace CrowdSimulation_OT_OOP
{
    /// <summary>
    /// Contains movement settings
    /// </summary>
    [CreateAssetMenu(menuName = "Flock/Settings/OOP_OT")]
    public class FlockSettings : ScriptableObject
    {
        [Header("Acceleration"), Space()]
        public float MinSpeed = 2;
        public float MaxSpeed = 5;
        public float MaxSteerForce = 3;

        [Header("Weights"), Space()]
        public float AlignWeight = 1;
        public float CohesionWeight = 1;
        public float SeperationWeight = 1;
        public float TargetWeight = 1;
        public float MoveToCenterDistance = 10f;
    }
}