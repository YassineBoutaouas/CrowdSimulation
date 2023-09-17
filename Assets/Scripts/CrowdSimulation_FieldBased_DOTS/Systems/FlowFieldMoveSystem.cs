using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Flowfield_DOTS
{
    /// <summary>
    /// Handles movement and flocking jobs executions and orders
    /// </summary>
    public partial struct FlockingSystem : ISystem
    {
        private RefRW<FlowFieldComponent> _flowField;
        private FlockAgentAspect.TypeHandle _aspectTypeHandle;

        [BurstCompile]
        void OnCreate(ref SystemState state)
        {
            _aspectTypeHandle = new FlockAgentAspect.TypeHandle(ref state);
        }

        [BurstCompile]
        void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW(out _flowField)) return;

            JobHandle createFlowFieldJobHandle;

            //Execute the job for calculating a flowfield only if the goal position has changed over the last frame
            if (!_flowField.ValueRW._previousPosition.Equals(_flowField.ValueRW.Goal.Position))
            {
                CreateFlowFieldJob createFlowField = new CreateFlowFieldJob(_flowField.ValueRW);

                createFlowFieldJobHandle = createFlowField.Schedule();

                createFlowFieldJobHandle.Complete();

                _flowField.ValueRW.Grid = createFlowField._flowField.Grid;

                _flowField.ValueRW._previousPosition = _flowField.ValueRW.Goal.Position;
            }

            state.CompleteDependency();

            //Calculate direction according to the flowfield
            GetDirectionToTargetJob getTargetDirections = new GetDirectionToTargetJob(_flowField.ValueRW);
            JobHandle moveJobHandle = getTargetDirections.Schedule(state.Dependency); //ScheduleParallel causes race conditions
            moveJobHandle.Complete();

            //Create and get a query containing all flock agents
            EntityQueryBuilder queryBuilder = new EntityQueryBuilder(Allocator.TempJob).WithAll<FlockAgentTagComponent>();
            EntityQuery entityQuery = state.GetEntityQuery(queryBuilder);

            _aspectTypeHandle.Update(ref state);

            //Apply velocities according to the behavioral model
            JobHandle flockingJobHandle = new FlockingJob(SystemAPI.Time.DeltaTime, _aspectTypeHandle).Schedule(entityQuery, moveJobHandle);

            flockingJobHandle.Complete();
        }
    }
}