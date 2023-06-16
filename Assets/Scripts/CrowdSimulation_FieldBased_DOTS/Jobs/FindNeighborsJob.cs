using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Flowfield_DOTS
{
    [BurstCompile]
    public partial struct FindNeighborsJob : IJobEntity, IDisposable
    {
        public QueryEnumerable<FlockAgentAspect> Agents;
        public float3 TargetDirection;
        public float DeltaTime;

        public FindNeighborsJob(float3 direction, float deltaTime, QueryEnumerable<FlockAgentAspect> agents)
        {
            TargetDirection = direction;
            DeltaTime = deltaTime;
            Agents = agents;
            __TypeHandle = default;
        }

        [BurstCompile]
        private float3 SteerTowards(float3 velocity, float maxSteerForce)
        {
            float len = math.clamp(math.length(velocity), 0, maxSteerForce);

            return math.normalize(velocity) * len;
        }

        [BurstCompile]
        public void Execute(FlockAgentAspect flockAgent)
        {
            //Find all neighbors
            //Calculate offsets for center, separation, alignment

            //Add together forces based on weights

            //Apply translation and rotation

            int numFlockMates = 0;

            float3 flockHeading = float3.zero;
            float3 separationHeading = float3.zero;

            float3 centerOfFlockMates = flockAgent.FlockAgent.ValueRO.CenterOfFlockmates;

            foreach (FlockAgentAspect agent in Agents)
            {
                if (agent.Transform.Equals(flockAgent.Transform)) continue;

                float3 offset = agent.Transform.ValueRO.Position - flockAgent.Transform.ValueRO.Position;
                float sqrDistance = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                if(sqrDistance < flockAgent.Settings.ValueRO.PerceptionRadius)
                {
                    numFlockMates++;
                    flockHeading += agent.FlockAgent.ValueRO.Forward;
                    centerOfFlockMates += agent.FlockAgent.ValueRO.Position;

                    if (sqrDistance < flockAgent.Settings.ValueRO.AvoidanceRadius)
                        separationHeading -= ((offset / sqrDistance));
                }
            }

            float3 acceleration = TargetDirection * flockAgent.Settings.ValueRO.TargetWeight;

            if (numFlockMates > 0)
            {
                centerOfFlockMates /= numFlockMates;

                float3 offsetToCenter = centerOfFlockMates - flockAgent.Transform.ValueRO.Position;

                float maxSteerForce = flockAgent.Settings.ValueRO.MaxSteerForce;
                acceleration += SteerTowards(flockHeading, maxSteerForce) * flockAgent.Settings.ValueRO.AlignWeight + SteerTowards(offsetToCenter, maxSteerForce) * flockAgent.Settings.ValueRO.CohesionWeight + SteerTowards(separationHeading, maxSteerForce) * flockAgent.Settings.ValueRO.SeparationWeight;
            }

            flockAgent.Transform.ValueRW = flockAgent.Transform.ValueRW.Translate(acceleration);
        }

        public void Dispose()
        {

        }
    }
}