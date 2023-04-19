using Boids_Lague;
using UnityEngine;
using UnityEngine.AI;

namespace CrowdSimulation
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class FlockAgent : MonoBehaviour
    {
        private FlockSettings _settings;
        private NavMeshAgent _agent;

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
        [HideInInspector]
        public int NumPerceivedFlockmates;

        private Vector3 _velocity;
        private Vector3 _acceleration;

        private Transform _cachedTransform;
        private Transform _target;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _cachedTransform = transform;
        }

        public void Initialize(FlockSettings settings, Transform target)
        {
            _target = target;
            _settings = settings;

            Position = _cachedTransform.position;
            Forward = _cachedTransform.forward;

            float startSpeed = (_settings.MinSpeed + _settings.MaxSpeed) / 2;
            _velocity = _cachedTransform.forward * startSpeed;
        }

        public void UpdateVelocity()
        {
            Vector3 acceleration = Vector3.zero;

            if(_target != null)
            {
                Vector3 offsetToTarget = (_target.position - Position);
                acceleration = SteerTowards(offsetToTarget) * _settings.TargetWeight;
            }

            if(NumPerceivedFlockmates != 0)
            {
                CenterOfFlockmates /= NumPerceivedFlockmates;

                Vector3 offsetToFlockMatesCenter = (CenterOfFlockmates - Position);

                var alignmentForce = SteerTowards(AvgFlockHeading) * _settings.AlignWeight;
                var cohesionForce = SteerTowards(offsetToFlockMatesCenter) * _settings.CohesionWeight;
                var seperationForce = SteerTowards(AvgAvoidanceHeading) * _settings.SeperationWeight;

                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += seperationForce;
            }

            if (IsHeadingForCollision())
            {
                Vector3 collisionAvoidanceDir = CalculateCollisionAvoidanceDir();
                Vector3 collisionAvoidanceForce = SteerTowards(collisionAvoidanceDir) * _settings.AvoidCollisionWeight;
                acceleration += collisionAvoidanceForce;
            }

            _velocity += acceleration * Time.deltaTime;
            float speed = _velocity.magnitude;
            Vector3 dir = _velocity.normalized;
            speed = Mathf.Clamp(speed, _settings.MinSpeed, _settings.MaxSpeed);
            _velocity = dir * speed;

            _agent.Move(_velocity * Time.deltaTime);
            _cachedTransform.forward = dir;
            Position = _cachedTransform.position;
            Forward = dir;
        }

        private bool IsHeadingForCollision()
        {
            if (Physics.SphereCast(Position, _settings.BoundsRadius, Forward, out RaycastHit _, _settings.CollisionAvoidanceDistance, _settings.ObstacleLayer, QueryTriggerInteraction.Ignore))
                return true;

            return false;
        }

        private Vector3 CalculateCollisionAvoidanceDir()
        {
            for (int i = 0; i < CollisionHelper.directions.Length; i++)
            {
                Vector3 dir = CollisionHelper.directions[i] * _cachedTransform.forward;
                if (!Physics.SphereCast(_cachedTransform.position + Vector3.up, _settings.BoundsRadius, dir, out RaycastHit hit, _settings.CollisionAvoidanceDistance, _settings.ObstacleLayer, QueryTriggerInteraction.Ignore))
                    return dir;
            }

            return Forward;
        }

        private Vector3 SteerTowards(Vector3 vector)
        {
            Vector3 v = vector.normalized * _settings.MaxSpeed - _velocity;
            return Vector3.ClampMagnitude(v, _settings.MaxSteerForce);
        }
    }

    public static class CollisionHelper
    {
        private const int numViewDirections = 10;
        private const int fieldOfView = 120;
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