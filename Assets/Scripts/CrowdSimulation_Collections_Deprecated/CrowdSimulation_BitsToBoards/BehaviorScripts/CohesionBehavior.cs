using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    [CreateAssetMenu(menuName = "Flock/Behavior/Cohesion", fileName = "New Cohesion Behavior")]
    public class CohesionBehavior : FilteredFlockBehavior
    {
        public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, FlockManager flock)
        {
            if(context.Count == 0)
                return Vector2.zero;

            Vector2 cohesionPos = Vector2.zero;
            List<Transform> filteredContext = (Filter == null) ? context : Filter.Filter(agent, context);
            foreach (Transform t in filteredContext)
                cohesionPos += (Vector2)t.position;

            cohesionPos /= context.Count;
            cohesionPos -= (Vector2)agent.transform.position;

            return cohesionPos;
        }
    }
}