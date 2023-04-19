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

        private void Awake()
        {
            Spawn();
        }

        public void Spawn()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                Vector2 pos = Random.insideUnitCircle * SpawnRadius;
                FlockAgent agent = Instantiate(AgentPrefabs[Random.Range(0, AgentPrefabs.Count)], new Vector3(pos.x, transform.position.y, pos.y), Quaternion.LookRotation(Random.insideUnitCircle, Vector3.up), transform);
                agent.name = $"Agent_{i}";

            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }
}