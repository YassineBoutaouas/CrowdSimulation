using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrowdSimulation_OOP
{
    public class GoalSelector : MonoBehaviour
    {
        public Transform[] Goals;
        private FlockSpawner _spawner;
        private int _goalIndex;

        private void Start()
        {
            _spawner = GetComponent<FlockSpawner>();
            _spawner.OnTargetReached += ChangeTarget;
        }

        public void ChangeTarget()
        {
            _goalIndex++;

            if (_goalIndex > Goals.Length - 1)
                _goalIndex = 0;

            _spawner._target.position = Goals[_goalIndex].position;
        }
    }
}