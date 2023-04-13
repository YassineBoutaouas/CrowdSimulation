using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    [CreateAssetMenu(menuName = "Flock/Behavior/SteeredCohesion", fileName = "New Steered Cohesion Behavior")]
    public class SteeredCohesionBehavior : FlockBehavior
    {
        public float AgentSmoothTime = 0.5f;

        private Vector2 _currentVelocity;

        public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, FlockManager flock)
        {
            if (context.Count == 0)
                return Vector2.zero;

            Vector2 cohesionPos = Vector2.zero;
            foreach (Transform t in context)
                cohesionPos += (Vector2)t.position;

            cohesionPos /= context.Count;
            cohesionPos -= (Vector2)agent.transform.position;

            return cohesionPos;
        }
    }
}