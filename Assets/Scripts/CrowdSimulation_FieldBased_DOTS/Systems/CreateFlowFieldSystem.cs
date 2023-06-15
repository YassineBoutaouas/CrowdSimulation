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
    ///[BurstCompile]
    ///public partial struct CreateFlowFieldSystem : ISystem
    ///{
    ///    public JobHandle FinalJobHandle;
    ///
    ///    private RefRW<FlowFieldComponent> _flowField;
    ///
    ///    [BurstCompile]
    ///    void OnCreate(ref SystemState state) { }
    ///
    ///    [BurstCompile]
    ///    void OnDestroy(ref SystemState state) { }
    ///
    ///    [BurstCompile]
    ///    void OnUpdate(ref SystemState state)
    ///    {
    ///        if (!SystemAPI.TryGetSingletonRW(out _flowField)) return;
    ///
    ///        state.CompleteDependency();
    ///        
    ///        if(!_flowField.ValueRW._previousPosition.Equals(_flowField.ValueRW.Goal.Position))
    ///        {
    ///            _flowField.ValueRW.IsCreated = false;
    ///
    ///            CreateFlowFieldJob createFlowField = new CreateFlowFieldJob(_flowField.ValueRW);
    ///
    ///            FinalJobHandle = createFlowField.Schedule();
    ///
    ///            FinalJobHandle.Complete();
    ///
    ///            _flowField.ValueRW.Grid = createFlowField._flowField.Grid;
    ///
    ///            createFlowField.Dispose();
    ///
    ///            _flowField.ValueRW._previousPosition = _flowField.ValueRW.Goal.Position;
    ///            _flowField.ValueRW.IsCreated = true;
    ///        }
    ///    }
    ///}
}