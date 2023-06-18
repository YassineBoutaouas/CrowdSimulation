using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Flowfield_DOTS
{
    [BurstCompile]
    public partial struct FlockingJob : IJobChunk, IDisposable
    {
        //public EntityQuery Agents;
        public float DeltaTime;

        public FlockAgentAspect.TypeHandle AspectTypeHandle;

        public FlockingJob(float deltaTime, FlockAgentAspect.TypeHandle typeHandle)
        {
            DeltaTime = deltaTime;
            AspectTypeHandle = typeHandle;

            //__TypeHandle = default;
        }

        [BurstCompile]
        private float3 SteerTowards(float3 velocity, float maxSteerForce)
        {
            float len = math.clamp(math.length(velocity), 0, maxSteerForce);

            if (len == 0)
                return float3.zero;

            return math.normalize(velocity) * len;

            //float3 direction = math.normalize(velocity);
            //float3 vec = direction * maxspeed - velocity;
            //return direction * math.clamp(math.length(vec), 0, maxSteerForce);
        }

        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            FlockAgentAspect.ResolvedChunk aspects = AspectTypeHandle.Resolve(chunk);

            for (int entityIndexA = 0; entityIndexA < aspects.Length; entityIndexA++)
            {
                int numFlockMates = 0;

                float3 centerOfFlockMates = float3.zero; //aspects[entityIndexB].FlockAgent.ValueRO.CenterOfFlockmates;
                float3 flockHeading = float3.zero;
                float3 separationHeading = float3.zero;

                for (int entityIndexB = 0; entityIndexB < aspects.Length; entityIndexB++)
                {
                    if (entityIndexA == entityIndexB) continue;

                    float3 offset = aspects[entityIndexB].Transform.ValueRO.Position - aspects[entityIndexA].Transform.ValueRO.Position;
                    float sqrDistance = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    if (sqrDistance < aspects[entityIndexA].Settings.ValueRO.PerceptionRadius)
                    {
                        numFlockMates++;
                        flockHeading += aspects[entityIndexB].FlockAgent.ValueRO.Forward;
                        centerOfFlockMates += aspects[entityIndexB].FlockAgent.ValueRO.Position;

                        if (sqrDistance < aspects[entityIndexA].Settings.ValueRO.AvoidanceRadius)
                            separationHeading -= offset / sqrDistance;
                    }
                }

                float maxSteerForce = aspects[entityIndexA].Settings.ValueRO.MaxSteerForce;

                float2 direction = aspects[entityIndexA].FlockAgent.ValueRO.CurrentDirection;

                float3 acceleration = SteerTowards(new float3(direction.x, 0, direction.y), maxSteerForce) * aspects[entityIndexA].Settings.ValueRO.TargetWeight; 

                if (numFlockMates > 0)
                {
                    centerOfFlockMates /= numFlockMates;

                    float3 offsetToCenter = centerOfFlockMates - aspects[entityIndexA].Transform.ValueRO.Position;

                    acceleration +=
                        SteerTowards(flockHeading, maxSteerForce) * aspects[entityIndexA].Settings.ValueRO.AlignWeight +
                        SteerTowards(offsetToCenter, maxSteerForce) * aspects[entityIndexA].Settings.ValueRO.CohesionWeight +
                        SteerTowards(separationHeading, maxSteerForce) * aspects[entityIndexA].Settings.ValueRO.SeparationWeight;
                }

                float3 velocity = acceleration * DeltaTime;
                float magnitude = math.length(velocity);

                if (magnitude > 0)
                {
                    float3 dir = math.normalize(velocity);
                    float speed = math.clamp(magnitude, aspects[entityIndexA].Settings.ValueRO.MinSpeed, aspects[entityIndexA].Settings.ValueRO.MaxSpeed);
                    velocity = dir * speed;

                    //Debug.Log($"velocity: {velocity}; acceleration:{acceleration}; heading: {flockHeading}; offsetCenter: {centerOfFlockMates}; separation: {separationHeading}");
                    //Debug.Log($"velocity: {velocity}; acceleration:{acceleration};");

                    aspects[entityIndexA].FlockAgent.ValueRW.Position = aspects[entityIndexA].Transform.ValueRO.Position;
                    aspects[entityIndexA].FlockAgent.ValueRW.Forward = dir;

                    //Set rotation
                    //-HERE-

                    //Set position
                    aspects[entityIndexA].Transform.ValueRW = aspects[entityIndexA].Transform.ValueRW.Translate(velocity * DeltaTime);
                }
            }
        }

        //[BurstCompile]
        //public void Execute(FlockAgentAspect flockAgent)
        //{
        //    //Find all neighbors
        //    //Calculate offsets for center, separation, alignment
        //
        //    //Add together forces based on weights
        //
        //    //Apply translation and rotation
        //
        //    int numFlockMates = 0;
        //
        //    float3 flockHeading = float3.zero;
        //    float3 separationHeading = float3.zero;
        //
        //    float3 centerOfFlockMates = flockAgent.FlockAgent.ValueRO.CenterOfFlockmates;
        //
        //    foreach (FlockAgentAspect agent in Agents.)
        //    {
        //        if (agent.Transform.Equals(flockAgent.Transform)) continue;
        //
        //        float3 offset = agent.Transform.ValueRO.Position - flockAgent.Transform.ValueRO.Position;
        //        float sqrDistance = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;
        //
        //        if(sqrDistance < flockAgent.Settings.ValueRO.PerceptionRadius)
        //        {
        //            numFlockMates++;
        //            flockHeading += agent.FlockAgent.ValueRO.Forward;
        //            centerOfFlockMates += agent.FlockAgent.ValueRO.Position;
        //
        //            if (sqrDistance < flockAgent.Settings.ValueRO.AvoidanceRadius)
        //                separationHeading -= ((offset / sqrDistance));
        //        }
        //    }
        //
        //    float3 acceleration = TargetDirection * flockAgent.Settings.ValueRO.TargetWeight;
        //
        //    if (numFlockMates > 0)
        //    {
        //        centerOfFlockMates /= numFlockMates;
        //
        //        float3 offsetToCenter = centerOfFlockMates - flockAgent.Transform.ValueRO.Position;
        //
        //        float maxSteerForce = flockAgent.Settings.ValueRO.MaxSteerForce;
        //        acceleration += SteerTowards(flockHeading, maxSteerForce) * flockAgent.Settings.ValueRO.AlignWeight + SteerTowards(offsetToCenter, maxSteerForce) * flockAgent.Settings.ValueRO.CohesionWeight + SteerTowards(separationHeading, maxSteerForce) * flockAgent.Settings.ValueRO.SeparationWeight;
        //    }
        //
        //    flockAgent.Transform.ValueRW = flockAgent.Transform.ValueRW.Translate(acceleration);
        //}

        public void Dispose()
        {

        }
    }
}