using Unity.Entities;
using UnityEngine;

namespace ECS_Temp_Testing
{
    public class RandomAuthoring : MonoBehaviour
    {

    }

    public class RandomBaker : Baker<RandomAuthoring>
    {
        public override void Bake(RandomAuthoring authoring)
        {
            var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
            AddComponent(entity, new RandomComponent { random = new Unity.Mathematics.Random(1) });
        }
    }

    public struct RandomComponent : IComponentData
    {
        public Unity.Mathematics.Random random;
    }
}