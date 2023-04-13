using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    public abstract class ContextFilter : ScriptableObject
    {
        public abstract List<Transform> Filter(FlockAgent agent, List<Transform> originals);
    }
}