using UnityEngine;
using UnityEngine.AI;

namespace CrowdSimulation_Shader
{
    /// <summary>
    /// Represents a single FlockAgent
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class FlockAgent : MonoBehaviour
    {
        #region public members
        public NavMeshPath PathToTarget;

        [HideInInspector]
        public Vector3 Acceleration;

        [HideInInspector]
        public Vector3 Position;
        [HideInInspector]
        public Vector3 Forward;

        [HideInInspector]
        public Vector3 CenterOfFlockmates;
        #endregion

        public Transform Target { get; private set; }

        private Transform _cachedTransform;
        private FlockSettings _settings;
        private NavMeshAgent _agent;
        private Vector3 _velocity;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _cachedTransform = transform;
        }

        public void Initialize(FlockSettings settings, Transform target)
        {
            PathToTarget = new NavMeshPath();

            Target = target;
            _settings = settings;

            Position = _cachedTransform.position;
            Forward = _cachedTransform.forward;

            _velocity = Vector3.zero;

            _agent.speed = Mathf.Lerp(_settings.MinSpeed, _settings.MaxSpeed, Random.Range(0f, 1f));
        }

        /// <summary>
        /// Calculate the total velocity
        /// </summary>
        public void UpdateVelocity()
        {
            _velocity = Acceleration * Time.deltaTime;
            Vector3 dir = _velocity.normalized;
            float speed = Mathf.Clamp(_velocity.magnitude, _settings.MinSpeed, _settings.MaxSpeed);
            _velocity = dir * speed;

            Position = _cachedTransform.position;
            _cachedTransform.forward = Vector3.Scale(Vector3.right + Vector3.forward, dir);
            Forward = dir;

            _agent.Move(_velocity * Time.deltaTime);
        }

        public void CalculatePath() { _agent.CalculatePath(Target.position, PathToTarget); }

        private void OnDrawGizmosSelected()
        {
            if (PathToTarget == null) return;
            if (PathToTarget.corners.Length == 0)
            {
                Debug.DrawLine(transform.position, Target.position, Color.grey);
                return;
            }

            for (int i = 0; i < PathToTarget.corners.Length; i++)
            {
                if (i + 1 >= PathToTarget.corners.Length) break;
                Debug.DrawLine(PathToTarget.corners[i], PathToTarget.corners[i + 1], Color.grey);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(CenterOfFlockmates, 1);
        }
    }
}