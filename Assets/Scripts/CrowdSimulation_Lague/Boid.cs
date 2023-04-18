using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Boids_Lague
{
    public class Boid : MonoBehaviour
    {
        private BoidSettings _settings;

        [HideInInspector]
        public Vector3 Position;
        [HideInInspector]
        public Vector3 Orientation;
        private Vector3 _velocity;

        private Vector3 _acceleration;
        [HideInInspector]
        public Vector3 AvgFlockHeading;
        [HideInInspector]
        public Vector3 AvgAvoidanceHeading;
        [HideInInspector]
        public Vector3 CenterOfFlockmates;
        [HideInInspector]
        public int NumPerceivedFlockmates;

        private Material _material;
        private Transform _target;
        private Transform _cachedTransform;

        private void Awake()
        {
            _material = GetComponentInChildren<Renderer>().material;
            _cachedTransform = transform;
        }

        public void Initialize(BoidSettings settings, Transform target)
        {
            _target = target;
            _settings = settings;

            Position = _cachedTransform.position;
            Orientation = _cachedTransform.up;

            _velocity = _cachedTransform.up * (settings.MinSpeed + settings.MaxSpeed) / 2;
        }

        public void SetColor(Color color) { if(_material!= null) _material.color = color; }

        public void UpdateBoid()
        {
            Vector3 acceleration = Vector3.zero;

            if (_target != null)
            {
                Vector3 offsetToTarget = (_target.position - Position);
                acceleration = SteerTowards(offsetToTarget) * _settings.TargetWeight;
            }

            if (NumPerceivedFlockmates != 0)
            {
                CenterOfFlockmates /= NumPerceivedFlockmates;

                Vector3 offsetToFlockMatesCenter = (CenterOfFlockmates - Position);

                Vector3 alignmentForce = SteerTowards(AvgFlockHeading) * _settings.AlignWeight;
                Vector3 cohesionForce = SteerTowards(offsetToFlockMatesCenter) * _settings.CohesionWeight;
                Vector3 seperationForce = SteerTowards(AvgAvoidanceHeading) * _settings.SeperateWeight;

                acceleration += alignmentForce;
                acceleration += cohesionForce;
                acceleration += seperationForce;
            }

            if (IsHeadingForCollision())
            {
                Vector3 collisionAvoidDir = ObstacleRays();
                Vector3 collisionAvoidForce = SteerTowards(collisionAvoidDir) * _settings.AvoidCollisionWeight;
                acceleration += collisionAvoidForce;
            }

            _velocity += acceleration * Time.deltaTime;
            float speed = _velocity.magnitude;
            Vector3 dir = _velocity.normalized;
            speed = Mathf.Clamp(speed, _settings.MinSpeed, _settings.MaxSpeed);
            _velocity = dir * speed;

            _cachedTransform.position += _velocity * Time.deltaTime;
            _cachedTransform.up = dir;
            Position = _cachedTransform.position;
            Orientation = dir;
        }

        private bool IsHeadingForCollision()
        {
            return Physics.SphereCast(Position, _settings.BoundsRadius, Orientation, out _, _settings.CollisionAvoidanceDistance, _settings.ObstacleMask);
        }

        Vector3 ObstacleRays()
        {
            Vector3[] rayDirections = BoidHelper.Directions;

            for (int i = 0; i < rayDirections.Length; i++)
            {
                Vector3 dir = transform.TransformDirection(rayDirections[i]);
                if (!Physics.SphereCast(Position, _settings.BoundsRadius, dir, out _, _settings.CollisionAvoidanceDistance, _settings.ObstacleMask))
                    return dir;
            }

            return Orientation;
        }

        private Vector3 SteerTowards(Vector3 vector)
        {
            Vector3 v = vector.normalized *  _settings.MaxSpeed - _velocity;
            return Vector3.ClampMagnitude(v, _settings.MaxSteerForce);
        }
    }
}
