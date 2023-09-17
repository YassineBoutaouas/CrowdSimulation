using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace CrowdSimulation_Shader
{
    /// <summary>
    /// Manages FlockAgent instances
    /// </summary>
    public class Flock : MonoBehaviour
    {
        public static Flock Instance;
        public List<FlockAgent> Agents = new List<FlockAgent>();
        public FlockSettings Settings;

        [Space()]
        private const int _threadGroupSize = 1024;
        public ComputeShader shader;
        private ComputeBuffer _agentBuffer;

        private void Awake() 
        { 
            Instance = this;

            //References a shader and passes initial values
            shader.SetFloat("viewRadius", Settings.PerceptionRadius);
            shader.SetFloat("avoidRadius", Settings.AvoidanceRadius);

            shader.SetFloat("maxSpeed", Settings.MaxSpeed);
            shader.SetFloat("maxSteerForce", Settings.MaxSteerForce);

            shader.SetFloat("alignWeight", Settings.AlignWeight);
            shader.SetFloat("cohesionWeight", Settings.CohesionWeight);
            shader.SetFloat("separationWeight", Settings.SeparationWeight); 
        }

        private void Update()
        {
            Profiler.BeginSample("Flock.Update");
            if (Agents.Count == 0) return;

            int numAgents = Agents.Count;
            AgentData[] agentData = new AgentData[numAgents];

            for (int i = 0; i < Agents.Count; i++)
            {
                agentData[i].Position = Agents[i].transform.position;
                agentData[i].Direction = Agents[i].Forward;
                
                Agents[i].CalculatePath();
                
                if(Agents[i].PathToTarget.corners.Length > 0)
                    agentData[i].TargetPosition = Agents[i].PathToTarget.corners[1];
            }

            //Creates a compute buffer and passes the values - paths are calculated on the main thread
            _agentBuffer = new ComputeBuffer(numAgents, AgentData.Size);
            _agentBuffer.SetData(agentData);

            shader.SetBuffer(0, "agents", _agentBuffer);

            shader.SetInt("numAgents", Agents.Count);

            int threadGroups = Mathf.CeilToInt(numAgents / (float)_threadGroupSize);

            shader.Dispatch(0, threadGroups, 1, 1);

            _agentBuffer.GetData(agentData);

            for(int i = 0; i < numAgents; i++)
            {
                Agents[i].Acceleration = agentData[i].Acceleration;
                Agents[i].CenterOfFlockmates = agentData[i].FlockCenter;
                Agents[i].UpdateVelocity();
            }

            _agentBuffer.Release();
            Profiler.EndSample();
        }

        private void OnDisable(){
            _agentBuffer.Release();
        }
    }

    /// <summary>
    /// To be passed to a shader. Contains the values held by a FlockAgent instance
    /// </summary>
    public struct AgentData
    {
        public Vector3 Position;
        public Vector3 Direction;

        public Vector3 FlockCenter;
        public Vector3 TargetPosition;

        public Vector3 Acceleration;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5;
            }
        }
    }
}