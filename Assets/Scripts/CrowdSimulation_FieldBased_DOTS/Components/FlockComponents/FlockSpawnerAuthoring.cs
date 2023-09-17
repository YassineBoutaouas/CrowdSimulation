using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Flowfield_DOTS
{
    /// <summary>
    /// MonoBehavior component containing spawn properties
    /// </summary>
    public class FlockSpawnerAuthoring : MonoBehaviour
    {
        public GameObject FlockAgentPrefab;
        public int SpawnCount;

        public float SpawnRadius;

        public void OnDrawGizmosSelected() { Gizmos.DrawWireSphere(transform.position, SpawnRadius); }
    }

    public class FlockSpawnerBaker : Baker<FlockSpawnerAuthoring>
    {
        public override void Bake(FlockSpawnerAuthoring authoring)
        {
            Entity spawner = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(spawner, new FlockSpawnerComponent(GetEntity(authoring.FlockAgentPrefab, TransformUsageFlags.Dynamic), authoring.SpawnCount, authoring.SpawnRadius, authoring.transform.position));
        }
    }

    /// <summary>
    /// DOTS component containing spawn properties for each flock
    /// </summary>
    public struct FlockSpawnerComponent : IComponentData
    {
        public Entity FlockAgentPrefab;
        public int SpawnCount;
        public float SpawnRadius;

        public int SpawnedInstances;

        public float3 SpawnPosition;

        public FlockSpawnerComponent(Entity flockAgent, int spawnCount, float spawnRadius, float3 spawnPos)
        {
            FlockAgentPrefab = flockAgent;
            SpawnCount = spawnCount;
            SpawnRadius = spawnRadius;

            SpawnedInstances = 0;

            SpawnPosition = spawnPos;
        }
    }
}