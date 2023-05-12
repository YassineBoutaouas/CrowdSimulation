using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace CrowdSimulation_Base
{
    [System.Serializable]
    public class BoxFormation : FormationBase
    {
        public int RowLength = 5;
        public bool Hollow = false;

        public override List<Vector3> EvaluatePoints(int spawnedAgents, Vector3 goal)
        {
            _currentFetchedPosition = 0;
            Positions.Clear();
            _spawnedAgents = spawnedAgents;

            int depth = Mathf.CeilToInt((float)_spawnedAgents / (float)RowLength);
            Vector3 middleOffset = new Vector3(RowLength * 0.5f * Spread, 0, depth * 0.5f * Spread);

            for (int i = 0; i < RowLength; i++)
            {
                for (int j = 0; j < depth; j++)
                {
                    if (i + j * RowLength > _spawnedAgents - 1) break;

                    if (Hollow && i != 0 && i != RowLength - 1 && j != 0 && j != depth - 1) continue;

                    Vector3 pos = new Vector3(i, 0, j);

                    pos += GetNoise(pos);
                    pos *= Spread;

                    pos += goal - middleOffset;
                    
                    NavMesh.SamplePosition(pos, out NavMeshHit hit, 20f, NavMesh.AllAreas);
                    Positions.Add(hit.position);
                }
            }

            return Positions;
        }
    }
}