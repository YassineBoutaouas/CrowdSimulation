using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Aspects;
using Unity.Transforms;
using UnityEngine;

namespace Flowfield_DOTS
{
    public partial class SpawnFlockSystem : SystemBase
    {
        private Unity.Mathematics.Random _random;

        private BeginSimulationEntityCommandBufferSystem.Singleton _beginSimulationCommanBufferSystem;

        private EntityCommandBuffer _buffer;

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            _random.InitState(12381234);
            _beginSimulationCommanBufferSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        protected override void OnUpdate()
        {
            _buffer = _beginSimulationCommanBufferSystem.CreateCommandBuffer(World.Unmanaged);

            foreach (RefRW<FlockSpawnerComponent> spawner in SystemAPI.Query<RefRW<FlockSpawnerComponent>>())
            {
                if (spawner.ValueRW.SpawnedInstances < spawner.ValueRW.SpawnCount)
                {
                    for (int i = spawner.ValueRW.SpawnedInstances; i < spawner.ValueRW.SpawnCount; i++)
                    {
                        Entity entity = _buffer.Instantiate(spawner.ValueRW.FlockAgentPrefab);

                        _buffer.SetComponent(entity, new LocalTransform
                        {
                            Position = new float3(
                            spawner.ValueRW.SpawnPosition.x + _random.NextFloat(0f, spawner.ValueRW.SpawnRadius),
                            spawner.ValueRW.SpawnPosition.y,
                            spawner.ValueRW.SpawnPosition.z + _random.NextFloat(0f, spawner.ValueRW.SpawnRadius)
                        ),
                            Scale = 1f
                        });

                        //PhysicsMass body = new PhysicsMass();
                        //body.Transform.pos = new float3(
                        //    spawner.ValueRW.SpawnPosition.x + _random.NextFloat(0f, spawner.ValueRW.SpawnRadius),
                        //    spawner.ValueRW.SpawnPosition.y,
                        //    spawner.ValueRW.SpawnPosition.z + _random.NextFloat(0f, spawner.ValueRW.SpawnRadius));
                        //
                        //_buffer.SetComponent(entity, body);

                        spawner.ValueRW.SpawnedInstances++;
                    }
                }
            }
        }
    }
}