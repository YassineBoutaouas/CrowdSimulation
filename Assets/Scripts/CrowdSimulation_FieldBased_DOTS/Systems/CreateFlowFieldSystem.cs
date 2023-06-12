using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Flowfield_DOTS
{
    [BurstCompile]
    public partial struct CreateFlowFieldSystem : ISystem
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

            JobHandle jobHandle;
            
            if(!_flowField.ValueRW._previousPosition.Equals(_flowField.ValueRW.Goal.Position))
            {
                CreateFlowFieldJob createFlowField = new CreateFlowFieldJob(_flowField.ValueRW.Grid, _flowField.ValueRW.GridSize, _flowField.ValueRW.GridOrigin, _flowField.ValueRW.CellRadius, _flowField.ValueRW.Goal.Position, _flowField.ValueRW.Directions);

                jobHandle = createFlowField.Schedule();

                jobHandle.Complete();

                _flowField.ValueRW.Grid = createFlowField.Grid;

                createFlowField.Dispose();

                _flowField.ValueRW._previousPosition = _flowField.ValueRW.Goal.Position;
            }

            
        }
    }
}