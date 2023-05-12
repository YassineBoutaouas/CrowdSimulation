using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    public abstract class FilteredFlockBehavior : FlockBehavior
    {
        public ContextFilter Filter;
    }
}