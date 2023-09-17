using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

namespace CrowdSimulation_OOP
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class FlockAgent : MonoBehaviour
    {
        private NavMeshPath _pathToTarget;
        private FlockSettings _settings;
        private NavMeshAgent _agent;
        private Transform _cachedTransform;

        [HideInInspector]
        public int NumPerceivedFlockmates;

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

        public Transform Target { get; private set; }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _cachedTransform = transform;
        }

        public void Initialize(FlockSettings settings, Transform target)
        {
            _pathToTarget = new NavMeshPath();

            Target = target;
            _settings = settings;

            Position = _cachedTransform.position;
            Forward = _cachedTransform.forward;

            float startSpeed = (_settings.MinSpeed + _settings.MaxSpeed) / 2;
            _velocity = _cachedTransform.forward * startSpeed;

            _agent.speed = Mathf.Lerp(_settings.MinSpeed, _settings.MaxSpeed, Random.Range(0f, 1f));
        }

        /// <summary>
        /// Calculates the velocity according to the behavioral model
        /// </summary>
        public void UpdateVelocity()
        {
            Profiler.BeginSample("FlockAgent.UpdateForces");

            Vector3 acceleration = Vector3.zero;

            if (Target != null) _agent.CalculatePath(Target.position, _pathToTarget);

            if (_pathToTarget.corners.Length >= 1)
            {
                Vector3 offsetToTarget = (_pathToTarget.corners[1] - Position);
                acceleration = SteerTowards(offsetToTarget) * _settings.TargetWeight;
            }

            //Calculate steering forces - alignment, cohesion, separation
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

            Profiler.EndSample();
        }

        /// <summary>
        /// Calculates the steering velocity
        /// </summary>
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