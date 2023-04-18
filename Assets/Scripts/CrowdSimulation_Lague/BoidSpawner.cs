using UnityEngine;

namespace Boids_Lague
{
    public class BoidSpawner : MonoBehaviour
    {
        public Boid Prefab;

        public float SpawnRadius = 10;
        public int SpawnCount = 10;
        public Color FlockColor;

        public Transform Goal;

        const int threadGroupSize = 1024;

        public BoidSettings Settings;
        public ComputeShader Shader;
        private Boid[] _boids;

        private void Awake()
        {
            _boids = new Boid[SpawnCount];

            for (int i = 0; i < SpawnCount; i++)
            {
                Boid boid = Instantiate(Prefab);
                boid.transform.position = transform.position + Random.insideUnitSphere * SpawnRadius;
                boid.transform.forward = Random.insideUnitSphere;

                boid.SetColor(FlockColor);

                _boids[i] = boid;
                boid.Initialize(Settings, Goal);
            }
        }

        private void Update()
        {
                var boidData = new BoidData[SpawnCount];

                for (int i = 0; i < _boids.Length; i++)
                {
                    boidData[i].Position = _boids[i].Position;
                    boidData[i].Orientation = _boids[i].Orientation;
                }

                var boidBuffer = new ComputeBuffer(SpawnCount, BoidData.Size);
                boidBuffer.SetData(boidData);

                Shader.SetBuffer(0, "boids", boidBuffer);
                Shader.SetInt("numBoids", _boids.Length);
                Shader.SetFloat("viewRadius", Settings.PerceptionRadius);
                Shader.SetFloat("avoidRadius", Settings.AvoidanceRadius);

                int threadGroups = Mathf.CeilToInt(SpawnCount / (float)threadGroupSize);
                Shader.Dispatch(0, threadGroups, 1, 1);

                boidBuffer.GetData(boidData);

                for (int i = 0; i < _boids.Length; i++)
                {
                    _boids[i].AvgFlockHeading = boidData[i].flockHeading;
                    _boids[i].CenterOfFlockmates = boidData[i].flockCentre;
                    _boids[i].AvgAvoidanceHeading = boidData[i].avoidanceHeading;
                    _boids[i].NumPerceivedFlockmates = boidData[i].numFlockmates;

                    _boids[i].UpdateBoid();
                }

                boidBuffer.Release();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = FlockColor * 0.8f;
            Gizmos.DrawWireSphere(transform.position, SpawnRadius);
        }
    }

    public struct BoidData
    {
        public Vector3 Position;
        public Vector3 Orientation;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size
        {
            get
            {
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }
}