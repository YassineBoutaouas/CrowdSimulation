using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace CrowdSimulation_OOP
{
    /// <summary>
    /// Manages FlockAgent instances and applies the flocking behaviors
    /// </summary>
    public class Flock : MonoBehaviour
    {
        public static Flock Instance;
        public List<FlockAgent> Agents = new List<FlockAgent>();

        [Header("NeighborIteration_Settings"), Space()]
        public float PerceptionRadius;
        public float AvoidanceRadius;

        private void Awake() { Instance = this; }

        private void Update()
        {
            if (Agents.Count == 0) return;

            Profiler.BeginSample("Flock.UpdateNeighbors");

            int numAgents = Agents.Count;

            //Bruteforce method O(n)^2
            //calculates alignment, cohesion, separation and a path
            for (int i = 0; i < numAgents; i++)
            {
                FlockAgent agentA = Agents[i];
                agentA.NumPerceivedFlockmates = 0;
                agentA.AvgFlockHeading = Vector3.zero;
                agentA.CenterOfFlockmates = Vector3.zero;
                agentA.AvgAvoidanceHeading = Vector3.zero;

                for (int j = 0; j < numAgents; j++)
                {
                    if (i == j) continue;

                    FlockAgent agentB = Agents[j];

                    Vector3 offset = agentB.Position - agentA.Position;
                    float sqrDistance = offset.x * offset.x + offset.y * offset.y + offset.z * offset.z;

                    if (sqrDistance < PerceptionRadius * PerceptionRadius)
                    {
                        agentA.NumPerceivedFlockmates++;
                        agentA.AvgFlockHeading += agentB.Forward;
                        agentA.CenterOfFlockmates += agentB.Position;

                        if (sqrDistance < AvoidanceRadius * AvoidanceRadius)
                            agentA.AvgAvoidanceHeading -= offset / sqrDistance;
                    }
                }

                agentA.UpdateVelocity();
            }

            Profiler.EndSample();
        }
    }
}