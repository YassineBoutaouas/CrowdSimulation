using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flocking_BitsToBoard
{
    [RequireComponent(typeof(Collider))]
    public class FlockAgent : MonoBehaviour
    {
        public FlockManager FlockManager { get; private set; }
        public Collider AgentCollider { private set; get; }

        private void Start()
        {
            AgentCollider = GetComponent<Collider>();
        }

        public void Initialize(FlockManager flock) { FlockManager = flock; }

        public void Move(Vector2 deltaPosition)
        {
            transform.up = deltaPosition;
            transform.position += (Vector3)deltaPosition * Time.deltaTime;
        }
    }
}
