using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Scripting;

namespace ECS_Temp_Testing
{
    [BurstCompile]
    public partial struct IMovingSystem : ISystem
    {
        [BurstCompile]
        void OnCreate(ref SystemState state)
        {
        }

        [BurstCompile]
        void OnDestroy(ref SystemState state)
        {

        }

        [BurstCompile]
        void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            RefRW<RandomComponent> ranRefRW = SystemAPI.GetSingletonRW<RandomComponent>();

            JobHandle jobHandle = new MoveJob { DeltaTime = deltaTime }.ScheduleParallel(state.Dependency);

            jobHandle.Complete();
            new TestReachedTargetPositionJob { randomRefRW = ranRefRW }.Run();
        }
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float DeltaTime;

        [BurstCompile]
        public void Execute(MoveToPositionAspect moveToPosition)
        {
            moveToPosition.MoveToTarget(DeltaTime);
        }
    }

    [BurstCompile]
    public partial struct TestReachedTargetPositionJob : IJobEntity
    {
        [NativeDisableUnsafePtrRestriction] public RefRW<RandomComponent> randomRefRW;

        [BurstCompile]
        public void Execute(MoveToPositionAspect moveToPosition)
        {
            moveToPosition.TestReachedTargetPosition(randomRefRW);
        }
    }
}