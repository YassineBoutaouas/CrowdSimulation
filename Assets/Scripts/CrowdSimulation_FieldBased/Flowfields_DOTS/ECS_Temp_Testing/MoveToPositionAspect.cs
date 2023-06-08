using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace ECS_Temp_Testing
{
    public readonly partial struct MoveToPositionAspect : IAspect
    {
        private readonly Entity _entity;

        private readonly RefRW<LocalTransform> _localTransform;
        private readonly RefRO<Speed> _speed;
        private readonly RefRW<TargetPosition> _targetPosition;

        public void MoveToTarget(float deltaTime)
        {
            float3 dir = math.normalize(_targetPosition.ValueRW.Value - _localTransform.ValueRW.Position);

            _localTransform.ValueRW = _localTransform.ValueRW.Translate(dir * _speed.ValueRO.Value * deltaTime);
        }

        public void TestReachedTargetPosition(RefRW<RandomComponent> random)
        {
            if (math.distancesq(_localTransform.ValueRW.Position, _targetPosition.ValueRW.Value) < 0.5f * 0.5f)
                _targetPosition.ValueRW.Value = GetRandomPosition(random);
        }

        public float3 GetRandomPosition(RefRW<RandomComponent> random)
        {
            return new float3(random.ValueRW.random.NextFloat(0f, 15f), random.ValueRW.random.NextFloat(0f, 15f), random.ValueRW.random.NextFloat(0f, 15f));
        }
    }
}