using Octree_Points;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace CrowdSimulation_OT_OOP
{
    /// <summary>
    /// Manages FlockAgent instances
    /// </summary>
    public class Flock : MonoBehaviour
    {
        public static Flock Instance;
        public List<FlockAgent> Agents = new List<FlockAgent>();

        public PointOctree Octree;

        [Header("Octree_Settings"), Space()]
        public float Size = 20f;
        public float MinNodeSize = 1f;

        [Header("NeighborIteration_Settings"), Space()]
        public float PerceptionRadius;
        public float AvoidanceRadius;

        private void Awake()
        {
            Instance = this;
            Octree = new PointOctree(Size, transform.position, MinNodeSize);
        }

        private void Update()
        {
            if (Agents.Count == 0) return;

            Profiler.BeginSample("Flock.UpdateNeighbors");

            int numAgents = Agents.Count;

            Octree = new PointOctree(Size, transform.position, MinNodeSize);

            for(int i = 0; i < numAgents; i++)
                Octree.Add(Agents[i], Agents[i].Position);

            for (int i = 0; i < numAgents; i++)
            {
                FlockAgent agentA = Agents[i];
                agentA.AvgFlockHeading = Vector3.zero;
                agentA.CenterOfFlockmates = Vector3.zero;
                agentA.AvgAvoidanceHeading = Vector3.zero;

                //Neighbor iteration handled by octree data structure - spatial partitioning may be considered for other crowd simulation approaches
                //O(Log n)
                Octree.GetNearby(agentA, agentA.Position, PerceptionRadius, AvoidanceRadius);
                               
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