using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;

namespace CrowdSimulation_MainThread_OOP
{

    public class Flock : MonoBehaviour
    {
        public static Flock Instance;
        //public static ProfilerCounterValue<int> frameCounter = new ProfilerCounterValue<int>("FrameCounter", ProfilerMarkerDataUnit.Count);
        public List<FlockAgent> Agents = new List<FlockAgent>();


        [Header("NeighborIteration_Settings"), Space()]
        public float PerceptionRadius;
        public float AvoidanceRadius;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (Agents.Count == 0) return;

            Profiler.BeginSample("Flock.UpdateNeighbors");

            int numAgents = Agents.Count;

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

    /// public struct AgentData
    /// {
    ///     public Vector3 Position;
    ///     public Vector3 Forward;
    ///
    ///     public Vector3 FlockHeading;
    ///     public Vector3 FlockCenter;
    ///     public Vector3 AvoidanceHeading;
    ///     public int NumFlockmates;
    ///
    ///     public static int Size
    ///     {
    ///         get
    ///         {
    ///             return sizeof(float) * 3 * 5 + sizeof(int); //For five vectors and one integer
    ///         }
    ///     }
    /// }
}