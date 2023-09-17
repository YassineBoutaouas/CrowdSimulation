using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Flowfield_DOTS
{
    /// <summary>
    /// Handles the spawning of flock agent instances
    /// </summary>
    public partial class SpawnFlockSystem : SystemBase
    {
        private Random _random;

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

            //Spawn flock agent instances for each spawner in the scene
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

                        spawner.ValueRW.SpawnedInstances++;
                    }
                }
            }
        }
    }
}