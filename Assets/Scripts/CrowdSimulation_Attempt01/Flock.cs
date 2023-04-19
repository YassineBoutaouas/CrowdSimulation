using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace CrowdSimulation 
{
    public class Flock : MonoBehaviour
    {
        public List<FlockAgent> AgentPrefabs = new List<FlockAgent>();

        public float SpawnRadius = 10;
        public int SpawnCount = 10;

        public List<FlockAgent> Agents = new List<FlockAgent>();

        [Space()]
        private const int _threadGroupSize = 1024;
        public FlockSettings Settings;
        public Transform Target;
        public ComputeShader shader;

        private void Awake()
        {
            Spawn();
        }

        public void Spawn()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                Vector2 pos = Random.insideUnitCircle * SpawnRadius;
                FlockAgent agent = Instantiate(AgentPrefabs[Random.Range(0, AgentPrefabs.Count)], new Vector3(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y), Quaternion.LookRotation(Random.insideUnitCircle, Vector3.up), transform);
                agent.name = $"Agent_{i}";
                agent.Initialize(Settings, Target);

                Agents.Add(agent);
            }
        }

        private void Update()
        {
            if (Agents.Count == 0) return;

            int numAgents = Agents.Count;
            AgentData[] agentData = new AgentData[numAgents];

            for (int i = 0; i < Agents.Count; i++)
            {
                agentData[i].Position = Agents[i].Position;
                agentData[i].Direction = Agents[i].Forward;
            }

            var agentBuffer = new ComputeBuffer(numAgents, AgentData.Size);
            agentBuffer.SetData(agentData);

            shader.SetBuffer(0, "agents", agentBuffer);
            shader.SetInt("numAgents", Agents.Count);
            shader.SetFloat("viewRadius", Settings.PerceptionRadius);
            shader.SetFloat("avoidRadius", Settings.AvoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numAgents / (float)_threadGroupSize);
            shader.Dispatch(0, threadGroups, 1, 1);

            agentBuffer.GetData(agentData);

            for (int i = 0; i < Agents.Count; i++)
            {
                Agents[i].AvgFlockHeading = agentData[i].FlockHeading;
                Agents[i].CenterOfFlockmates = agentData[i].FlockCenter;
                Agents[i].AvgAvoidanceHeading = agentData[i].AvoidanceHeading;
                Agents[i].NumPerceivedFlockmates = agentData[i].NumFlockmates;

                Agents[i].UpdateVelocity();
            }

            agentBuffer.Release();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }

    public struct AgentData
    {
        public Vector3 Position;
        public Vector3 Direction;

        public Vector3 FlockHeading;
        public Vector3 FlockCenter;
        public Vector3 AvoidanceHeading;
        public int NumFlockmates;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int); //For five vectors and one integer
            }
        }
    }
}