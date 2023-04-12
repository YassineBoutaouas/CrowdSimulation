using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour
{
    public GameObject Goal;
    private NavMeshAgent _agent;

    private void Start()
    {
        _agent = gameObject.AddComponent<NavMeshAgent>();
        _agent.speed = 10f;
        _agent.acceleration = 10f;
        _agent.SetDestination(Goal.transform.position);
    }
}
