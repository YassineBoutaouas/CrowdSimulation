using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace CrowdSimulation_Base
{
    [System.Serializable]
    public abstract class FormationBase
    {
        protected int _spawnedAgents;
        [Range(0f, 1f)]
        public float Noise = 0;
        public float Spread = 1;

        protected int _currentFetchedPosition;

        public List<Vector3> Positions = new List<Vector3>();

        public abstract List<Vector3> EvaluatePoints(int spawnedAgents, Vector3 goal);

        public Vector3 GetNoise(Vector3 pos)
        {
            float noise = Mathf.PerlinNoise(pos.x * Noise, pos.z * Noise);

            return new Vector3(noise, pos.y, noise);
        }

        public Vector3 GetNext(Vector3 originalPos)
        {
            if(_currentFetchedPosition > Positions.Count - 1) return originalPos;
            Vector3 p = Positions[_currentFetchedPosition];
            _currentFetchedPosition++;
            return p;
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < Positions.Count; i++)
            {
                Gizmos.DrawSphere(Positions[i], 1);
            }
        }
    }
}