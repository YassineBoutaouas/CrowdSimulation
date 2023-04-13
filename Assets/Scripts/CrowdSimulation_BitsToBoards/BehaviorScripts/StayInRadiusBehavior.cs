using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard 
{
    [CreateAssetMenu(menuName = "Flock/Behavior/StayInRadius")]
    public class StayInRadiusBehavior : FlockBehavior
    {
        public Vector2 Center;
        public float Radius = 15f;

        public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, FlockManager flock)
        {
            Vector2 centerOffset = Center - (Vector2)agent.transform.position;
            float t = centerOffset.magnitude / Radius;
            if(t < 0.9f)
                return Vector2.zero;

            return t * t * centerOffset;
        }
    }
}