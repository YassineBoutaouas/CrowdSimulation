using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    public class FlockManager : MonoBehaviour
    {
        public FlockAgent AgentPrefab;
        public FlockBehavior flockBehavior;
        private List<FlockAgent> FlockAgents = new List<FlockAgent>();

        [Range(10, 500)]
        public int StartingCount = 250;
        private const float AgentDensity = 0.08f;

        [Range(1f, 100f)]
        public float DriveFactor = 10f;
        [Range(1f, 100f)]
        public float MaxSpeed = 5f;

        [Range(1f, 10f)]
        public float NeighborRadius = 1.5f;
        [Range(0f, 1f)]
        public float AvoidanceRadiusMultiplier = 0.5f;

        private float _squareMaxSpeed;
        private float _squareNeighborRadius;
        public float SquareAvoidanceRadius { private set; get; }

        private void Start()
        {
            _squareMaxSpeed = MaxSpeed * MaxSpeed;
            _squareNeighborRadius = NeighborRadius * NeighborRadius;
            SquareAvoidanceRadius = _squareNeighborRadius * AvoidanceRadiusMultiplier * AvoidanceRadiusMultiplier;

            for (int i = 0; i < StartingCount; i++)
            {
                FlockAgent newAgent = Instantiate(
                    AgentPrefab,
                    AgentDensity * StartingCount * Random.insideUnitCircle,
                    Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                    transform
                    );
                newAgent.name = "Agent_" + i;
                FlockAgents.Add(newAgent);
            }
        }

        private void Update()
        {
            foreach (FlockAgent agent in FlockAgents)
            {
                List<Transform> context = GetNearbyObjects(agent);
                Vector2 move = flockBehavior.CalculateMove(agent, context, this);
                move *= DriveFactor;
                if (move.sqrMagnitude > _squareMaxSpeed)
                    move = move.normalized * MaxSpeed;

                agent.Move(move);

                //agent.GetComponentInChildren<Renderer>().material.color = Color.Lerp(Color.white, Color.red, context.Count / 6f);
            }
        }

        private List<Transform> GetNearbyObjects(FlockAgent agent)
        {
            List<Transform> context = new List<Transform>();
            Collider[] contextColliders = Physics.OverlapSphere(agent.transform.position, NeighborRadius);
            foreach (Collider collider in contextColliders)
            {
                if (collider != agent.AgentCollider)
                    context.Add(collider.transform);
            }

            return context;
        }
    }
}