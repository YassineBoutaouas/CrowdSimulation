using Flowfield;
using Global;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
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

            //Dependencies:

            state.CompleteDependency();

            ///ExportPhysicsWorld:CheckDynamicBodyIntegrity
            ///Jobs:CreateRigidBodies
            ///Jobs:RecordDynamicBodyIntegrity

            //JobHandle jobHandle = new GetCellFromWorldPositionJob(_flowField.ValueRW.Grid, _flowField.ValueRW.GridSize, _flowField.ValueRW.GridOrigin, _flowField.ValueRW.CellRadius, _flowField.ValueRW.Goal.Position).ScheduleParallel(state.Dependency);
            JobHandle moveJobHandle = new GetCellFromWorldPositionJob(_flowField.ValueRW).Schedule(state.Dependency);

            moveJobHandle.Complete();
        }
    }
}