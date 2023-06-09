using CrowdSimulation_DOTS;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Flowfield_DOTS
{
    public readonly partial struct FlockAgentAspect : IAspect
    {
        private readonly Entity _entity;

        private readonly RefRW<LocalTransform> _localTransform;
        private readonly RefRO<FlockAgentTag> _agentTag;

        //public void Spawn(RefRW<RandomComponent> random, float3 originPos, float spawnRadius)
        //{
        //    float3 pos = new float3(originPos.x + random.ValueRW.Random.NextFloat(spawnRadius), originPos.y, originPos.z + random.ValueRW.Random.NextFloat(spawnRadius));
        //
        //    _localTransform.ValueRW = _localTransform.ValueRW.Translate(pos);
        //}
    }
}