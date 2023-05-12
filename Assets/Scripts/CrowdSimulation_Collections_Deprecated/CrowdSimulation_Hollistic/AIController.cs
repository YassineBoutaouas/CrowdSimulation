using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Hollistic_Crowd
{

    public class AIController : MonoBehaviour
    {
        private NavMeshAgent _agent;

        private Animator _animator;
        private int _movement;

        private void Start()
        {
            if (!TryGetComponent(out _agent))
                _agent = gameObject.AddComponent<NavMeshAgent>();

            _animator = GetComponentInChildren<Animator>();

            SetDestination();

            _movement = Animator.StringToHash("ForwardSpeed");
        }

        private void SetDestination()
        {
            _agent.SetDestination(GoalManager.Instance.Goals[Random.Range(0, GoalManager.Instance.Goals.Count)].transform.position);
        }

        private void Update()
        {
            if (_agent.remainingDistance < _agent.stoppingDistance)
                SetDestination();

            _animator.SetFloat(_movement, _agent.velocity.magnitude);
        }
    }
}