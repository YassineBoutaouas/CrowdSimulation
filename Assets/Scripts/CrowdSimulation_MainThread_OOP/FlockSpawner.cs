using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdSimulation_OOP
{
    public class FlockSpawner : MonoBehaviour
    {
        public List<FlockAgent> AgentPrefabs = new List<FlockAgent>();

        public float SpawnRadius = 10;
        public int SpawnCount = 10;

        public FlockSettings Settings;
        public Transform _target;
        private Vector3 _previousTargetPos;

        public event Action OnTargetPositionChanged;
        public event Action OnTargetReached;
        public void TargetReached(){ OnTargetReached?.Invoke(); }

        private void Start()
        {
            if (_target != null)
            {
                _previousTargetPos = _target.position;
            }

            Spawn();
        }

        private void Update()
        {
            if(_target == null) return;
            
            if (_previousTargetPos == _target.position) return;
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
                agent.Initialize(Settings, this, _target, i);

                OnTargetPositionChanged += agent.ChangeTargetState;

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