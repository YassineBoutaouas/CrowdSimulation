using UnityEngine;
using UnityEngine.AI;

namespace CrowdSimulation.Scenarios.Testing
{
    public class ScenarioTesting : MonoBehaviour
    {
        public GameObject[] AgentPrefabs;

        public float Radius = 10f;
        public int NumAgents = 10;

        public Vector3 SpawnOffset;
        public Vector3 GoalOffset;

        public NavMeshAgent[] Agents;
        public Transform[] Goals;

        public float TimeScale = 1f;

        private void Awake()
        {
            Time.timeScale = TimeScale;
        }

        public void SpawnAgentsInCircle()
        {
            float angle = 360f / (float)NumAgents;
            float yPos = 1;

            Agents = new NavMeshAgent[NumAgents];
            Goals = new Transform[NumAgents];

            GameObject goalParent = new GameObject("Goals");
            goalParent.transform.parent = transform;
            goalParent.transform.localPosition = Vector3.zero;

            float radians = 2 * Mathf.PI / NumAgents;

            for (int i = 0; i < NumAgents; i++)
            {
                float x = transform.position.x + SpawnOffset.x + Radius * Mathf.Sin(radians * i);
                float z = transform.position.x + SpawnOffset.x + Radius * Mathf.Cos(radians * i);

                GameObject agent = Instantiate(AgentPrefabs[Random.Range(0, AgentPrefabs.Length)]);
                agent.transform.position = new Vector3(x, yPos, z);

                GameObject goal = new GameObject();
                goal.transform.position = agent.transform.position;

                agent.transform.parent = transform;
                goal.transform.parent = goalParent.transform;

                Agents[i] = agent.GetComponent<NavMeshAgent>();
                Goals[i] = goal.transform;
            }
        }

        public void SpawnAgentsInLine()
        {
            Vector3 startPos = transform.position + GoalOffset + (Vector3.right * (-NumAgents / 2));
            GameObject goalParent = new GameObject("Goals");
            goalParent.transform.parent = transform;
            goalParent.transform.localPosition = GoalOffset;

            Agents = new NavMeshAgent[NumAgents];
            Goals = new Transform[NumAgents];

            for (int i = 0; i < NumAgents; i++)
            {
                GameObject agent = Instantiate(AgentPrefabs[Random.Range(0, AgentPrefabs.Length)]);
                agent.transform.position = transform.position + SpawnOffset;

                GameObject goal = new GameObject($"Goal_{i}");
                goal.transform.position = startPos + (i * Vector3.right);

                agent.transform.parent = transform;
                goal.transform.parent = goalParent.transform;

                Agents[i] = agent.GetComponent<NavMeshAgent>();
                Goals[i] = goal.transform;
            }
        }

        public void ReverseDistribution()
        {
            for (int i = 0; i < Goals.Length; i++)
            {
                Agents[(Agents.Length - 1) - i].SetDestination(Goals[i].position);
            }
        }

        public void LinearDistribution()
        {
            for (int i = 0; i < Goals.Length; i++)
            {
                Agents[i].SetDestination(Goals[i].position);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position + SpawnOffset, Radius);
            Gizmos.DrawWireSphere(transform.position + GoalOffset, Radius / 2);
        }
    }
}