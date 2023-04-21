
using System.Collections.Generic;
using UnityEngine;

namespace CrowdSimulation
{
    public class FlockSpawner : MonoBehaviour
    {
        public List<FlockAgent> AgentPrefabs = new List<FlockAgent>();

        public float SpawnRadius = 10;
        public int SpawnCount = 10;

        public FlockSettings Settings;
        public Transform Target;

        private void Start()
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