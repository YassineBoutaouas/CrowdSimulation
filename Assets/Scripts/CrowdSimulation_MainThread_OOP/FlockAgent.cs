using Global.Positioning;
using UnityEngine;
using UnityEngine.AI;

namespace CrowdSimulation_MainThread_OOP
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class FlockAgent : MonoBehaviour
    {
        private Animator _animator;
        private NavMeshPath _pathToTarget;
        private FlockSettings _settings;
        private NavMeshAgent _agent;
        private Transform _cachedTransform;

        #region Direction and Acceleration
        [HideInInspector]
        public Vector3 Position;
        [HideInInspector]
        public Vector3 Forward;

        [HideInInspector]
        public Vector3 AvgFlockHeading;
        [HideInInspector]
        public Vector3 AvgAvoidanceHeading;
        [HideInInspector]
        public Vector3 CenterOfFlockmates;

        private Vector3 _velocity;
        #endregion

        [HideInInspector]
        public int NumPerceivedFlockmates;

        public Transform Target { get; private set; }
        public bool _hasReachedTarget { get; private set; }

        private BoxFormation _boxFormation;
        private int _positionIndex;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _cachedTransform = transform;
        }

        public void Initialize(FlockSettings settings, Transform target, BoxFormation boxFormation, int positionIndex)
        {
            _pathToTarget = new NavMeshPath();
            _boxFormation = boxFormation;
            _positionIndex = positionIndex;

            _animator = GetComponent<Animator>();

            Target = target;
            _settings = settings;

            Position = _cachedTransform.position;
            Forward = _cachedTransform.forward;

            float startSpeed = (_settings.MinSpeed + _settings.MaxSpeed) / 2;
            _velocity = _cachedTransform.forward * startSpeed;

            _agent.speed = Mathf.Lerp(_settings.MinSpeed, _settings.MaxSpeed, Random.Range(0f, 1f));
        }

        public void UpdateVelocity()
        {
            Vector3 acceleration = Vector3.zero;

            if (Target != null)
            {
                if (Vector3.Distance(transform.position, Target.position) < _settings.MoveToCenterDistance)
                {
                    _agent.ResetPath();
                    _hasReachedTarget = true;
                    _agent.SetDestination(_boxFormation.Positions[_positionIndex]);
                }

                if (_hasReachedTarget) return;

                _agent.CalculatePath(Target.position, _pathToTarget);
            }

            if (_pathToTarget.corners.Length >= 1)
            {
                Vector3 offsetToTarget = (_pathToTarget.corners[1] - Position);
                acceleration = SteerTowards(offsetToTarget) * _settings.TargetWeight;
            }

            if (NumPerceivedFlockmates != 0)
            {
                CenterOfFlockmates /= NumPerceivedFlockmates;

                Vector3 offsetToFlockMatesCenter = (CenterOfFlockmates - Position);

                Vector3 alignmentForce = SteerTowards(AvgFlockHeading) * _settings.AlignWeight;
                Vector3 cohesionForce = SteerTowards(offsetToFlockMatesCenter) * _settings.CohesionWeight;
                Vector3 seperationForce = SteerTowards(AvgAvoidanceHeading) * _settings.SeperationWeight;

                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += seperationForce;
            }

            _velocity += acceleration * Time.deltaTime;
            float speed = _velocity.magnitude;
            Vector3 dir = _velocity.normalized;
            speed = Mathf.Clamp(speed, _settings.MinSpeed, _settings.MaxSpeed);
            _velocity = dir * speed;

            Position = _cachedTransform.position;
            _cachedTransform.forward = Vector3.Scale(Vector3.right + Vector3.forward, dir);
            Forward = dir;

            _agent.Move(_velocity * Time.deltaTime);
        }

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
            if (Target != null)
            {
                if (_pathToTarget == null) return;
                if (_pathToTarget.corners.Length == 0)
                {
                    Debug.DrawLine(transform.position, Target.position, Color.grey);
                    return;
                }

                for (int i = 0; i < _pathToTarget.corners.Length; i++)
                {
                    if (i + 1 >= _pathToTarget.corners.Length) break;
                    Debug.DrawLine(_pathToTarget.corners[i], _pathToTarget.corners[i + 1], Color.grey);
                }
            }

            Gizmos.color = Color.magenta + Color.grey;
            Gizmos.DrawSphere(CenterOfFlockmates, 1);
        }
    }
}