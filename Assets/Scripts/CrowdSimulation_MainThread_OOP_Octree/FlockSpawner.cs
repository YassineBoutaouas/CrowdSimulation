using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdSimulation_OT_OOP
{
    /// <summary>
    /// Spawns FlockAgent instances
    /// </summary>
    public class FlockSpawner : MonoBehaviour
    {
        public List<FlockAgent> AgentPrefabs = new List<FlockAgent>();

        public float SpawnRadius = 10;
        public int SpawnCount = 10;

        public FlockSettings Settings;
        public Transform _target;

        private void Start() { Spawn(); }

        public void Spawn()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                Vector2 pos = UnityEngine.Random.insideUnitCircle * SpawnRadius;
                FlockAgent agent = Instantiate(AgentPrefabs[UnityEngine.Random.Range(0, AgentPrefabs.Count)], new Vector3(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y), Quaternion.LookRotation(UnityEngine.Random.insideUnitCircle, Vector3.up), transform);
                agent.name = $"Agent_{i}";
                agent.Initialize(Settings, this, _target);

                Flock.Instance.Agents.Add(agent);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }
}