using Unity.Entities;
using Unity.Transforms;

namespace Flowfield_DOTS
{
    public readonly partial struct FlockAgentAspect : IAspect
    {
        private readonly Entity _entity;

        public readonly RefRW<LocalTransform> Transform;
        public readonly RefRO<FlockAgentTagComponent> TagComponent;
    }
}