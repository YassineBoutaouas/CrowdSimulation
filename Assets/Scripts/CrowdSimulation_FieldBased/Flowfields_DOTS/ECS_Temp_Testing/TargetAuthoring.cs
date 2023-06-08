using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ECS_Temp_Testing
{
    public class TargetAuthoring : MonoBehaviour
    {
        public float3 Value;
    }

    public class TargetPositionBaker : Baker<TargetAuthoring>
    {
        public override void Bake(TargetAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new TargetPosition { Value = authoring.Value });
        }
    }

    public struct TargetPosition : IComponentData
    {
        public float3 Value;
    }
}