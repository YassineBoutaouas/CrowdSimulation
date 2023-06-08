using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECS_Temp_Testing
{
    public class SpeedAuthoring : MonoBehaviour
    {
        public float Value;


    }

    public class SpeedBaker : Baker<SpeedAuthoring>
    {
        public override void Bake(SpeedAuthoring authoring)
        {
            // Accessing the transform using Baker function, not the GameObject one
            // so the this baker can keep track of the dependency
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new Speed { Value = authoring.Value });
        }
    }

    public struct Speed : IComponentData
    {
        public float Value;
    }
}