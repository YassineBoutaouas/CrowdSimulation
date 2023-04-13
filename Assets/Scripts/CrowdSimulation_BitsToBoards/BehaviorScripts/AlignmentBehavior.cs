using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    [CreateAssetMenu(menuName = "Flock/Behavior/Alignment", fileName = "New Alignment Behavior")]
    public class AlignmentBehavior : FlockBehavior
    {
        public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, FlockManager flock)
        {
            if (context.Count == 0)
                return agent.transform.up;

            Vector2 alignmentPos = Vector2.zero;
            foreach (Transform t in context)
                alignmentPos += (Vector2)t.up;

            alignmentPos /= context.Count;

            return alignmentPos;
        }
    }
}