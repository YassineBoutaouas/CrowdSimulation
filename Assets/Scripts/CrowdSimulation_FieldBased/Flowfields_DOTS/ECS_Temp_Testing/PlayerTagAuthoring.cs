using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace ECS_Temp_Testing
{
    public class PlayerTagAuthoring : MonoBehaviour
    {

    }

    public class PlayerTagBaker : Baker<PlayerTagAuthoring>
    {
        public override void Bake(PlayerTagAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PlayerTag());
        }
    }

    public struct PlayerTag : IComponentData
    {

    }
}