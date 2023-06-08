using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECS_Temp_Testing
{
    public partial class PlayerSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            EntityQuery playerQuery = EntityManager.CreateEntityQuery(typeof(PlayerTag));

            PlayerSpawnerComponent playerSpawner = SystemAPI.GetSingleton<PlayerSpawnerComponent>();
            RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();

            EntityCommandBuffer entityBuffer = 
                SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);

            if (playerQuery.CalculateEntityCount() < playerSpawner.SpawnCount)
            {
                Entity e = entityBuffer.Instantiate(playerSpawner.entity);
                //EntityManager.Instantiate(playerSpawner.entity);

                entityBuffer.SetComponent(e, new Speed { Value = randomComponent.ValueRW.random.NextFloat(2f, 10f) });
            }
        }
    }
}