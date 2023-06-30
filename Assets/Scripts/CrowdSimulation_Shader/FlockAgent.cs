using UnityEngine;
using UnityEngine.AI;

namespace CrowdSimulation_Shader
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class FlockAgent : MonoBehaviour
    {
        private Transform _cachedTransform;
        public Transform Target { get; private set; }

        private Animator _animator;
        public bool _hasReachedTarget { get; private set; }
        
        public NavMeshPath PathToTarget;
        private FlockSettings _settings;
        private NavMeshAgent _agent;

        [HideInInspector]
        public Vector3 Acceleration;

        [HideInInspector]
        public Vector3 Position;
        [HideInInspector]
        public Vector3 Forward;

        [HideInInspector]
        public Vector3 CenterOfFlockmates;

        private Vector3 _velocity;
        private Vector3 _acceleration;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _cachedTransform = transform;
        }

        public void Initialize(FlockSettings settings, Transform target, int positionIndex)
        {
            PathToTarget = new NavMeshPath();

            _animator = GetComponent<Animator>();

            Target = target;
            _settings = settings;

            Position = _cachedTransform.position;
            Forward = _cachedTransform.forward;

            float startSpeed = (_settings.MinSpeed + _settings.MaxSpeed) / 2;
            _velocity = Vector3.zero; //_cachedTransform.forward * startSpeed;

            _agent.speed = Mathf.Lerp(_settings.MinSpeed, _settings.MaxSpeed, Random.Range(0f, 1f));
        }

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

        public void ChangeTargetState()
        {
            _agent.isStopped = false;
            _agent.ResetPath();
            _hasReachedTarget = false;
        }

        private Vector3 SteerTowards(Vector3 vector)
        {
            Vector3 v = vector.normalized * _settings.MaxSpeed - _velocity;
            return Vector3.ClampMagnitude(v, _settings.MaxSteerForce);
        }

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

            // Gizmos.color = Color.cyan;
            // Gizmos.DrawSphere(_pathToTarget.corners[1], 1);

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(CenterOfFlockmates, 1);
        }
    }
}