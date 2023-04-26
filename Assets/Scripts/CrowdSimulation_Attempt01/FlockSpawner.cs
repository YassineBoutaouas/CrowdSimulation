using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace CrowdSimulation
{
    public class FlockSpawner : MonoBehaviour
    {
        public List<FlockAgent> AgentPrefabs = new List<FlockAgent>();

        public float SpawnRadius = 10;
        public int SpawnCount = 10;

        public FlockSettings Settings;
        public Transform _target;
        private Vector3 _previousTargetPos;

        public BoxFormation _boxFormation;

        public event Action OnTargetPositionChanged;

        private void Start()
        {
            _previousTargetPos = _target.position;
            _boxFormation.EvaluatePoints(SpawnCount, _target.position);
            Spawn();
        }

        private void Update()
        {
            if (_previousTargetPos == _target.position) return;

            _boxFormation.EvaluatePoints(SpawnCount, _target.position);
            _previousTargetPos = _target.position;

            OnTargetPositionChanged?.Invoke();
        }

        public void Spawn()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                Vector2 pos = UnityEngine.Random.insideUnitCircle * SpawnRadius;
                FlockAgent agent = Instantiate(AgentPrefabs[UnityEngine.Random.Range(0, AgentPrefabs.Count)], new Vector3(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y), Quaternion.LookRotation(UnityEngine.Random.insideUnitCircle, Vector3.up), transform);
                agent.name = $"Agent_{i}";
                agent.Initialize(Settings, _target, _boxFormation, i);

                OnTargetPositionChanged += agent.ChangeTargetState;

                Flock.Instance.Agents.Add(agent);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            _boxFormation.OnDrawGizmosSelected();
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }
}