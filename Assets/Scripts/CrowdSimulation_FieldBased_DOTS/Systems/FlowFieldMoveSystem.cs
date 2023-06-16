using Flowfield;
using Global;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Flowfield_DOTS
{
    public partial struct FlowFieldMoveSystem : ISystem
    {
        private RefRW<FlowFieldComponent> _flowField;

        [BurstCompile]
        void OnCreate(ref SystemState state) { }

        [BurstCompile]
        void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonRW(out _flowField)) return;

            state.CompleteDependency();

            JobHandle createFlowFieldJobHandle;

            //Execute the job for calculating a flowfield - only do it if the goal position has changed over the last frame
            if (!_flowField.ValueRW._previousPosition.Equals(_flowField.ValueRW.Goal.Position))
            {
                _flowField.ValueRW.IsCreated = false;

                CreateFlowFieldJob createFlowField = new CreateFlowFieldJob(_flowField.ValueRW);

                createFlowFieldJobHandle = createFlowField.Schedule();

                createFlowFieldJobHandle.Complete();

                _flowField.ValueRW.Grid = createFlowField._flowField.Grid;

                createFlowField.Dispose();

                _flowField.ValueRW._previousPosition = _flowField.ValueRW.Goal.Position;
                _flowField.ValueRW.IsCreated = true;
            }

            state.CompleteDependency();

            //ScheduleParallel causes race conditions
            GetCellFromWorldPositionJob getTargetDirection = new GetCellFromWorldPositionJob(_flowField.ValueRW);
            JobHandle moveJobHandle = getTargetDirection.Schedule(state.Dependency);

            moveJobHandle.Complete();

            float3 direction = new float3(getTargetDirection.Direction[0], 0f, getTargetDirection.Direction[1]);

            
            //EntityQuery entityQuery;

            JobHandle flockingJobHandle = new FindNeighborsJob(direction, SystemAPI.Time.DeltaTime,  .Schedule(state.Dependency);

            flockingJobHandle.Complete();
        }
    }
}