using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;
using Octree_Points;

namespace CrowdSimulation_OT_OOP
{

    public class Flock : MonoBehaviour
    {
        public static Flock Instance;
        //public static ProfilerCounterValue<int> frameCounter = new ProfilerCounterValue<int>("FrameCounter", ProfilerMarkerDataUnit.Count);
        public List<FlockAgent> Agents = new List<FlockAgent>();

        public PointOctree<FlockAgent> Octree;

        [Header("Octree_Settings"), Space()]
        public float Size = 20f;
        public float MinNodeSize = 1f;

        [Header("NeighborIteration_Settings"), Space()]
        public float PerceptionRadius;
        public float AvoidanceRadius;

        private void Awake()
        {
            Instance = this;
            Octree = new PointOctree<FlockAgent>(Size, transform.position, MinNodeSize);
        }

        private void Update()
        {
            if (Agents.Count == 0) return;

            Profiler.BeginSample("Flock.UpdateNeighbors");

            int numAgents = Agents.Count;

            //Build octree
            //Fetch neighbors for each agent
            //Proceed with operations

            for (int i = 0; i < numAgents; i++)
            {
                Octree.Remove(Agents[i]);
                Octree.Add(Agents[i], Agents[i].Position);
            }


            for (int i = 0; i < numAgents; i++)
            {
                FlockAgent agentA = Agents[i];
                agentA.AvgFlockHeading = Vector3.zero;
                agentA.CenterOfFlockmates = Vector3.zero;
                agentA.AvgAvoidanceHeading = Vector3.zero;

                FlockAgent[] flockNeighbors = Octree.GetNearbyToArray(agentA.Position, PerceptionRadius);

                for (int j = 0; j < flockNeighbors.Length; j++)
                {
                    FlockAgent agentB = flockNeighbors[j];

                    if (agentA == agentB) continue;

                    Vector3 offset = agentB.Position - agentA.Position;
                    float sqrDistance = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    agentA.AvgFlockHeading += agentB.Forward;
                    agentA.CenterOfFlockmates += agentB.Position;

                    if (sqrDistance < AvoidanceRadius * AvoidanceRadius)
                        agentA.AvgAvoidanceHeading -= offset / sqrDistance;
                }
                               
                agentA.UpdateVelocity();
            }

            Profiler.EndSample();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red * Color.blue;
            if (!Application.isPlaying)
                Gizmos.DrawWireCube(transform.position, Vector3.one * Size);

            if (Octree != null)
                Octree.DrawAllBounds();
        }
    }
}