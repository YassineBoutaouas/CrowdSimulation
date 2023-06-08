using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
 
namespace ECS_Temp_Testing
{
    public class PlayerSpawnerAuthoring : MonoBehaviour
    {
        public GameObject PlayerPrefab;
        public int SpawnCount;
    }

    public class PlayerSpawnerBaker : Baker<PlayerSpawnerAuthoring>
    {
        public override void Bake(PlayerSpawnerAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerSpawnerComponent(GetEntity(authoring.PlayerPrefab, TransformUsageFlags.Dynamic), authoring.SpawnCount));
        }
    }

    public struct PlayerSpawnerComponent : IComponentData
    {
        public Entity entity;
        public int SpawnCount;

        public PlayerSpawnerComponent(Entity e, int spawnCount)
        {
            entity = e;
            SpawnCount = spawnCount;
        }
    }
}