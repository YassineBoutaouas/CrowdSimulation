using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace CrowdSimulation_DOTS
{
    public class FlockAgentAuthoring : MonoBehaviour { }

    public class FlockAgentTagBaker : Baker<FlockAgentAuthoring>
    {
        public override void Bake(FlockAgentAuthoring authoring)
        {
            Entity entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new FlockAgentTag());
        }
    }

    public struct FlockAgentTag : IComponentData { }
}