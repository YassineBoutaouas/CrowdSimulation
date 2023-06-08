using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

namespace ECS_Temp_Testing
{
    public partial class MovingSystemBase : SystemBase
    {
        protected override void OnUpdate()
        {
            //foreach (MoveToPositionAspect moveToPosition in SystemAPI.Query<MoveToPositionAspect>())
            //{
            //    RefRW<RandomComponent> randomRefRW = SystemAPI.GetSingletonRW<RandomComponent>();
            //
            //    moveToPosition.MoveToTarget(SystemAPI.Time.DeltaTime);
            //    moveToPosition.TestReachedTargetPosition(randomRefRW);
            //}

            ///foreach (
            ///    (RefRW<LocalTransform> transform, RefRO<Speed> speed, RefRW<TargetPosition> position) in 
            ///    SystemAPI.Query<RefRW<LocalTransform>, RefRO<Speed>, RefRW<TargetPosition>>())
            ///{
            ///    float3 dir = math.normalize(position.ValueRW.Value - transform.ValueRW.Position);
            ///
            ///    transform.ValueRW = transform.ValueRW.Translate(dir * speed.ValueRO.Value * SystemAPI.Time.DeltaTime);
            ///}

            ///Entities.ForEach((ref LocalTransform transform) =>
            ///{
            ///    transform = transform.Translate(new float3(1 * SystemAPI.Time.DeltaTime, 0,0));
            ///}).Run();
        }
    }
}