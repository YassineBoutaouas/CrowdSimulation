//using System.Collections;
//using System.Collections.Generic;
//using Unity.Burst;
//using Unity.Entities;
//using UnityEngine;

//namespace Flowfield_DOTS
//{
//    public partial class SpawnFlockSystem : SystemBase
//    {
//        private EntityQuery _flockEntityQuery;
//        private FlockSpawnerComponent _flockSpawnerComponent;
//        private RefRW<RandomComponent> _randomComponent;

//        private BeginSimulationEntityCommandBufferSystem.Singleton _beginSimulationCommanBufferSystem;

//        protected override void OnStartRunning()
//        {
//            base.OnStartRunning();
//            _flockEntityQuery = EntityManager.CreateEntityQuery(typeof(FlockAgentTagComponent));
//        }

//        protected override void OnUpdate()
//        {
            
//        }
//    }
//}