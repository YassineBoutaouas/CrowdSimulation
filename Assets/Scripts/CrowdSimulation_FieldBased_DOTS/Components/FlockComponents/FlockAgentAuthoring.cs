using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Flowfield_DOTS
{
    /// <summary>
    /// MonoBehavior component containing FlockAgent data
    /// </summary>
    public class FlockAgentAuthoring : MonoBehaviour
    {
        public float3 Position;
        public float3 Forward;

        public float3 AvgFlockHeading;
        public float3 AvgAvoidanceHeading;
        public float3 CenterOfFlockmates;

        public float3 Velocity;


        public float Speed;
    }

    public class FlockAgentBaker : Baker<FlockAgentAuthoring>
    {
        public override void Bake(FlockAgentAuthoring authoring)
        {
            Entity e = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(e, new FlockAgentComponent(authoring.Position, authoring.Forward, authoring.AvgFlockHeading, authoring.AvgAvoidanceHeading, authoring.CenterOfFlockmates, authoring.Velocity));
        }
    }

    /// <summary>
    /// DOTS component containing FlockAgent data
    /// </summary>
    public struct FlockAgentComponent : IComponentData
    {
        public float2 CurrentDirection;
        public float3 Position;
        public float3 Forward;

        public float3 AvgFlockHeading;
        public float3 AvgAvoidanceHeading;
        public float3 CenterOfFlockmates;

        public float3 Velocity;

        public FlockAgentComponent(float3 position, float3 forward, float3 avgflockheading, float3 avgavoidanceheading, float3 centerofflockmates, float3 velocity)
        {
            CurrentDirection = float2.zero;
            Position = position;
            Forward = forward;
            AvgFlockHeading = avgflockheading;
            AvgAvoidanceHeading = avgavoidanceheading;
            CenterOfFlockmates = centerofflockmates;
            Velocity = velocity;
        }
    }
}