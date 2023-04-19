using UnityEngine;
using UnityEngine.AI;

namespace CrowdSimulation
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class FlockAgent : MonoBehaviour
    {
        private FlockSettings _settings;

        [HideInInspector]
        public Vector3 Position;
        [HideInInspector]
        public Vector3 Forward;

        private Vector3 _velocity;

        private void Awake()
        {

        }

        public void Initialize(FlockSettings settings, Transform target)
        {

        }

        private void OnDrawGizmosSelected()
        {

        }
    }

    public static class CollisionHelper
    {
        const int numViewDirections = 10;
        const int fieldOfView = 120;
        public static readonly Quaternion[] directions;

        static CollisionHelper()
        {
            directions = new Quaternion[numViewDirections];

            float angleIncrement = fieldOfView / numViewDirections;
            float fovOffset = -(angleIncrement * numViewDirections / 2);
            for (int i = 0; i < numViewDirections; i++)
            {
                Quaternion angleDirection = Quaternion.AngleAxis(fovOffset + (angleIncrement * i), Vector3.up);
                directions[i] = angleDirection;
            }
        }
    }
}