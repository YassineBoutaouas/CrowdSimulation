using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    [CreateAssetMenu(menuName = "Flock/Filter/SameFlock")]
    public class SameFlockFilter : ContextFilter
    {
        public override List<Transform> Filter(FlockAgent agent, List<Transform> originals)
        {
            List<Transform> filtered = new List<Transform>();
            foreach (Transform t in originals)
            {
                 if(t.TryGetComponent(out FlockAgent neighbor))
                    if(neighbor.FlockManager == agent.FlockManager)
                        filtered.Add(t);
            }

            return filtered;
        }
    }
}