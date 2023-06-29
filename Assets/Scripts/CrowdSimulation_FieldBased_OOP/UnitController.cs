using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;

namespace Flowfield
{
    public class UnitController : MonoBehaviour
    {
        public GridController Grid;
        public GameObject UnitPrefab;
        public int SpawnCount;
        public float SpawnRadius;
        public float MoveSpeed;

        public ProfilerMarker ProfilerMarker;

        public List<NavMeshAgent> UnitsInGame = new List<NavMeshAgent>();
        public List<NavMeshPath> Paths = new List<NavMeshPath>();

        private void Start()
        {
            ProfilerMarker = new ProfilerMarker("Flowfield.Pathfind");
            SpawnUnits();
        }

        private void SpawnUnits()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                Vector2 pos = UnityEngine.Random.insideUnitCircle * SpawnRadius;
                GameObject agent = Instantiate(UnitPrefab, new Vector3(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y), Quaternion.LookRotation(UnityEngine.Random.insideUnitCircle, Vector3.up), transform);
                agent.name = $"Agent_{i}";

                UnitsInGame.Add(agent.GetComponent<NavMeshAgent>());
                Paths.Add(new NavMeshPath());
            }
        }

        private void Update()
        {
            ProfilerMarker.Begin();

            foreach (NavMeshAgent obj in UnitsInGame)
            {
                Cell nodeBelow = Grid.CurrentFlowField.GetCellFromWorldPosition(obj.transform.position);
                Vector3 move = new Vector3(nodeBelow.BestDirection.Vector.x, 0, nodeBelow.BestDirection.Vector.y);
            
                obj.transform.position += Time.deltaTime * MoveSpeed * move;
            }

            //for(int i = 0; i< UnitsInGame.Count; i++)
            //{
            //    UnitsInGame[i].CalculatePath(Grid.Goal.position, Paths[i]);
            //
            //    UnitsInGame[i].SetPath(Paths[i]);
            //}

            ProfilerMarker.End();
        }
    }
}