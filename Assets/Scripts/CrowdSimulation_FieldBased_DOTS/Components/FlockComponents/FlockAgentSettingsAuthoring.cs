using Unity.Entities;
using UnityEngine;

namespace Flowfield_DOTS
{
    /// <summary>
    /// MonoBehavior component containing FlockAgent settings
    /// </summary>
    public class FlockAgentSettingsAuthoring : MonoBehaviour
    {
        [Header("Acceleration"), Space()]
        public float Speed = 1;
        public float MinSpeed = 2;
        public float MaxSpeed = 5;
        public float MaxSteerForce = 3;

        [Header("Avoidance"), Space()]
        public float AvoidanceRadius;
        public float PerceptionRadius;

        [Header("Weights"), Space()]
        public float AlignWeight = 1;
        public float CohesionWeight = 1;
        public float SeparationWeight = 1;
        public float TargetWeight = 1;
    }

    public class FlockSettingsBaker : Baker<FlockAgentSettingsAuthoring>
    {
        public override void Bake(FlockAgentSettingsAuthoring authoring)
        {
            Entity e = GetEntity(authoring, TransformUsageFlags.Dynamic);
            authoring.Speed = UnityEngine.Random.Range(authoring.MinSpeed, authoring.MaxSpeed);
            AddComponent(e, new FlockAgentSettingsComponent(authoring.MinSpeed, authoring.MaxSpeed, authoring.MaxSteerForce, authoring.AvoidanceRadius, authoring.PerceptionRadius, authoring.AlignWeight, authoring.CohesionWeight, authoring.SeparationWeight, authoring.TargetWeight, authoring.Speed));
        }
    }

    /// <summary>
    /// DOTS component containing settings and values such as avoidance and speed
    /// </summary>
    public struct FlockAgentSettingsComponent : IComponentData
    {
        public float Speed;
        public float MinSpeed;
        public float MaxSpeed;
        public float MaxSteerForce;

        public float AvoidanceRadius;
        public float PerceptionRadius;

        public float AlignWeight;
        public float CohesionWeight;
        public float SeparationWeight;
        public float TargetWeight;

        public FlockAgentSettingsComponent(float minSpeed, float maxSpeed, float maxSteerForce, float avoidanceRadius, float perceptionRadius, float alignWeight, float cohesionweight, float separationWeight, float targetWeight, float speed)
        {
            MinSpeed = minSpeed;
            MaxSpeed = maxSpeed;
            MaxSteerForce = maxSteerForce;

            AvoidanceRadius = avoidanceRadius * avoidanceRadius;
            PerceptionRadius = perceptionRadius * perceptionRadius;

            AlignWeight = alignWeight;
            CohesionWeight = cohesionweight;
            SeparationWeight = separationWeight;
            TargetWeight = targetWeight;

            Speed = speed;
        }
    }
}