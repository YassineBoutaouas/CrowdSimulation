using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    [CreateAssetMenu(menuName = "Flock/Behavior/Avoidance", fileName = "New Avoidance Behavior")]
    public class AvoidanceBehavior : FilteredFlockBehavior
    {
        public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, FlockManager flock)
        {
            if (context.Count == 0)
                return Vector2.zero;

            Vector2 avoidancePos = Vector2.zero;
            int neighborsToavoide = 0;
            List<Transform> filteredContext = (Filter == null) ? context : Filter.Filter(agent, context);
            foreach (Transform t in filteredContext)
                if (Vector2.SqrMagnitude(t.position - agent.transform.position) < flock.SquareAvoidanceRadius)
                {
                    neighborsToavoide++;
                    avoidancePos += (Vector2)(agent.transform.position - t.position);
                }

            if(neighborsToavoide > 0)
                avoidancePos /= neighborsToavoide;

            return avoidancePos;
        }
    }
}