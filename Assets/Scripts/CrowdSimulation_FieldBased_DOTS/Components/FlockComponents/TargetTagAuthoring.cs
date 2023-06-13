using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Flowfield_DOTS
{
    public class TargetTagAuthoring : MonoBehaviour { }

    public class TargetTagBaker : Baker<TargetTagAuthoring>{
        public override void Bake(TargetTagAuthoring authoring)
        {
            Entity e = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(e, new TargetTagComponent());
        }
    }

    public struct TargetTagComponent : IComponentData { }
}