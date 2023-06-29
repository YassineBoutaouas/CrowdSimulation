using Flowfield;
using Global;
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flowfield_DOTS
{
    public partial struct FlockingSystem : ISystem
    {
        private RefRW<FlowFieldComponent> _flowField;
        private FlockAgentAspect.TypeHandle _aspectTypeHandle;

        //private EntityQueryBuilder queryBuílder;

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

            //Execute the job for calculating a flowfield - only do it if the goal position has changed over the last frame
            if (!_flowField.ValueRW._previousPosition.Equals(_flowField.ValueRW.Goal.Position))
            {
                CreateFlowFieldJob createFlowField = new CreateFlowFieldJob(_flowField.ValueRW);

                createFlowFieldJobHandle = createFlowField.Schedule();

                createFlowFieldJobHandle.Complete();

                _flowField.ValueRW.Grid = createFlowField._flowField.Grid;

                _flowField.ValueRW._previousPosition = _flowField.ValueRW.Goal.Position;
            }

            state.CompleteDependency();

            //ScheduleParallel causes race conditions
            GetDirectionToTargetJob getTargetDirections = new GetDirectionToTargetJob(_flowField.ValueRW);
            JobHandle moveJobHandle = getTargetDirections.Schedule(state.Dependency);
            moveJobHandle.Complete();

            EntityQueryBuilder queryBuilder = new EntityQueryBuilder(Allocator.TempJob).WithAll<FlockAgentTagComponent>();
            EntityQuery entityQuery = state.GetEntityQuery(queryBuilder);
            //state.

            _aspectTypeHandle.Update(ref state);

            JobHandle flockingJobHandle = new FlockingJob(SystemAPI.Time.DeltaTime, _aspectTypeHandle).Schedule(entityQuery, moveJobHandle); //.Schedule(state.Dependency);

            flockingJobHandle.Complete();
        }
    }
}