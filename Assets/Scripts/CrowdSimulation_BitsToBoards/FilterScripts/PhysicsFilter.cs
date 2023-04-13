using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    [CreateAssetMenu(menuName = "Flock/Filter/PhysicsFilter")]
    public class PhysicsFilter : ContextFilter
    {
        public LayerMask ObstacleLayer;
        public override List<Transform> Filter(FlockAgent agent, List<Transform> originals)
        {
            List<Transform> filtered = new List<Transform>();
            foreach (Transform t in originals)
            {
                if (ObstacleLayer == (ObstacleLayer | (1 << t.gameObject.layer)))
                    filtered.Add(t);
            }

            return filtered;
        }
    }
}