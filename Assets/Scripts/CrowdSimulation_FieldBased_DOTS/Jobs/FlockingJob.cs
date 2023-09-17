using System;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Flowfield_DOTS
{
    /// <summary>
    /// Calculates and returns the velocity of each FlockAgent according to the behavioral model
    /// </summary>
    [BurstCompile]
    public partial struct FlockingJob : IJobChunk, IDisposable
    {
        public float DeltaTime;

        public FlockAgentAspect.TypeHandle AspectTypeHandle;

        public FlockingJob(float deltaTime, FlockAgentAspect.TypeHandle typeHandle)
        {
            DeltaTime = deltaTime;
            AspectTypeHandle = typeHandle;
        }

        /// <summary>
        /// Steering method to calculate the total velocity
        /// </summary>
        /// <returns></returns>
        [BurstCompile]
        private float3 SteerTowards(float3 velocity, float maxSteerForce)
        {
            float len = math.clamp(math.length(velocity), 0, maxSteerForce);

            if (len == 0)
                return float3.zero;

            return math.normalize(velocity) * len;
        }

        /// <summary>
        /// Iterates over each archetype chunk to access and modify each FlockAgentAspect
        /// </summary>
        [BurstCompile]
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            FlockAgentAspect.ResolvedChunk aspects = AspectTypeHandle.Resolve(chunk);

            //Bruteforce algorithm O(n)^2 -> spatial partitioning may further improve the performance
            for (int entityIndexA = 0; entityIndexA < aspects.Length; entityIndexA++)
            {
                int numFlockMates = 0;

                float3 centerOfFlockMates = float3.zero;
                float3 flockHeading = float3.zero;
                float3 separationHeading = float3.zero;

                //Calculate the velocities for separation, alignment and cohesion
                for (int entityIndexB = 0; entityIndexB < aspects.Length; entityIndexB++)
                {
                    if (entityIndexA == entityIndexB) continue;

                    float3 offset = aspects[entityIndexB].Transform.ValueRO.Position - aspects[entityIndexA].Transform.ValueRO.Position;
                    float sqrDistance = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    if (sqrDistance < aspects[entityIndexA].Settings.ValueRO.PerceptionRadius)
                    {
                        numFlockMates++;
                        flockHeading += aspects[entityIndexB].FlockAgent.ValueRO.Forward;
                        centerOfFlockMates += aspects[entityIndexA].Transform.ValueRO.Position;

                        if (sqrDistance < aspects[entityIndexA].Settings.ValueRO.AvoidanceRadius)
                            separationHeading -= offset / sqrDistance;
                    }
                }

                float maxSteerForce = aspects[entityIndexA].Settings.ValueRO.MaxSteerForce;

                //Set the initial direction to the previously found path orientation
                float2 direction = aspects[entityIndexA].FlockAgent.ValueRO.CurrentDirection * aspects[entityIndexA].Settings.ValueRO.Speed * aspects[entityIndexA].Settings.ValueRO.Speed;

                //acceleration = goaldirection + alignment + cohesion + separation
                float3 acceleration = SteerTowards(new float3(direction.x, 0, direction.y) * aspects[entityIndexA].Settings.ValueRO.Speed, maxSteerForce) * aspects[entityIndexA].Settings.ValueRO.TargetWeight;

                if (numFlockMates > 0)
                {
                    centerOfFlockMates /= numFlockMates;
                
                    float3 offsetToCenter = centerOfFlockMates - aspects[entityIndexA].Transform.ValueRO.Position;
                
                    acceleration +=
                        SteerTowards(flockHeading, maxSteerForce) * aspects[entityIndexA].Settings.ValueRO.AlignWeight +
                        SteerTowards(offsetToCenter, maxSteerForce) * aspects[entityIndexA].Settings.ValueRO.CohesionWeight +
                        SteerTowards(separationHeading, maxSteerForce) * aspects[entityIndexA].Settings.ValueRO.SeparationWeight;
                }

                acceleration *= DeltaTime;
                float magnitude = math.length(acceleration);

                //Set orientation and position of each agent according to velocity
                if (magnitude > 0)
                {
                    float3 dir = math.normalize(acceleration);
                    float speed = math.clamp(magnitude, aspects[entityIndexA].Settings.ValueRO.MinSpeed, aspects[entityIndexA].Settings.ValueRO.MaxSpeed);
                    acceleration = dir * speed;

                    aspects[entityIndexA].FlockAgent.ValueRW.Forward = dir;

                    aspects[entityIndexA].Transform.ValueRW = new LocalTransform { Position = aspects[entityIndexA].Transform.ValueRO.Position + acceleration * DeltaTime, Rotation = Quaternion.LookRotation(dir), Scale = 1 };
                }
            }
        }

        public void Dispose() { }
    }
}