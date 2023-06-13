using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace Flowfield_DOTS
{
    public class RandomAuthoring : MonoBehaviour { }

    public class RandomBaker : Baker<RandomAuthoring>
    {
        public override void Bake(RandomAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new RandomComponent { Random = new Unity.Mathematics.Random(1) });
        }
    }

    public struct RandomComponent : IComponentData
    {
        public Unity.Mathematics.Random Random;
    }
}