using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    public abstract class FlockBehavior : ScriptableObject
    {
        public abstract Vector2 CalculateMove(FlockAgent agent, List<Transform> context, FlockManager flock);
    }
}