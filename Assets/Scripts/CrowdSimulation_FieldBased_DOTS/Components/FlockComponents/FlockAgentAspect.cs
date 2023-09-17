using Unity.Entities;
using Unity.Transforms;

namespace Flowfield_DOTS
{
    /// <summary>
    /// Aspect storing references to the DOTS components: 
    /// Transform, Settings, FlockAgent, TagComponent
    /// </summary>
    public readonly partial struct FlockAgentAspect : IAspect
    {
        public readonly RefRW<LocalTransform> Transform;

        public readonly RefRO<FlockAgentSettingsComponent> Settings;
        public readonly RefRW<FlockAgentComponent> FlockAgent;

        public readonly RefRO<FlockAgentTagComponent> TagComponent;
    }
}