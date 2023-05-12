using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    [CreateAssetMenu(menuName = "Flock/Behavior/Composite", fileName = "New Composite Behavior")]
    public class CompositeBehavior : FlockBehavior
    {
        public WeightedFlockBehavior[] Behaviors;

        public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, FlockManager flock)
        {
            if (Behaviors.Length == 0)
                return Vector2.zero;

            Vector2 move = Vector2.zero;
            for (int i = 0; i < Behaviors.Length; i++)
            {
                Vector2 partialMove = Behaviors[i].flockBehavior.CalculateMove(agent, context, flock) * Behaviors[i].Weight;
                if (partialMove != Vector2.zero)
                {
                    if(partialMove.sqrMagnitude > Behaviors[i].Weight * Behaviors[i].Weight) 
                    {
                        partialMove.Normalize();
                        partialMove *= Behaviors[i].Weight;
                    }

                    move += partialMove;
                }
            }

            return move;
        }
    }

    [System.Serializable]
    public class WeightedFlockBehavior
    {
        public float Weight;
        public FlockBehavior flockBehavior;
    }
}