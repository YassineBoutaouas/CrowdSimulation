using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Flowfield_DOTS
{
    public class FlockSpawnerAuthoring : MonoBehaviour
    {
        public GameObject FlockAgentPrefab;
        public int SpawnCount;

        public float SpawnRadius;

        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }

    public class FlockSpawnerBaker : Baker<FlockSpawnerAuthoring>
    {
        public override void Bake(FlockSpawnerAuthoring authoring)
        {
            Entity spawner = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(spawner, new FlockSpawnerComponent(GetEntity(authoring.FlockAgentPrefab, TransformUsageFlags.Dynamic), authoring.SpawnCount, authoring.SpawnRadius));
        }
    }

    public struct FlockSpawnerComponent : IComponentData
    {
        public Entity FlockAgentPrefab;
        public int SpawnCount;
        public float SpawnRadius;

        public FlockSpawnerComponent(Entity flockAgent, int spawnCount, float spawnRadius)
        {
            FlockAgentPrefab = flockAgent;
            SpawnCount = spawnCount;
            SpawnRadius = spawnRadius;
        }
    }
}