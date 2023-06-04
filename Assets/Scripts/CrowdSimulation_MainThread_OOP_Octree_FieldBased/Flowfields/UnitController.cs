using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flowfield
{
    public class UnitController : MonoBehaviour
    {
        public GridController Grid;
        public GameObject UnitPrefab;
        public int SpawnCount;
        public float SpawnRadius;
        public float MoveSpeed;

        private List<GameObject> UnitsInGame = new List<GameObject>();

        private void Start()
        {
            SpawnUnits();
        }

        private void SpawnUnits()
        {
            for (int i = 0; i < SpawnCount; i++)
            {
                Vector2 pos = UnityEngine.Random.insideUnitCircle * SpawnRadius;
                GameObject agent = Instantiate(UnitPrefab, new Vector3(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y), Quaternion.LookRotation(UnityEngine.Random.insideUnitCircle, Vector3.up), transform);
                agent.name = $"Agent_{i}";

                UnitsInGame.Add(agent);
            }
        }

        private void Update()
        {
            foreach (GameObject obj in UnitsInGame)
            {
                Cell nodeBelow = Grid.CurrentFlowField.GetCellFromWorldPosition(obj.transform.position);
                Vector3 move = new Vector3(nodeBelow.BestDirection.Vector.x, 0, nodeBelow.BestDirection.Vector.y);

                obj.transform.position += Time.deltaTime * MoveSpeed * move;
            }
        }
    }
}