using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Flowfield_DOTS
{
    public class FlockAgentTagAuthoring : MonoBehaviour { }

    public class FlockAgentTagBaker : Baker<FlockAgentTagAuthoring>
    {
        public override void Bake(FlockAgentTagAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlockAgentTagComponent());
        }
    }

    public struct FlockAgentTagComponent : IComponentData { }
}