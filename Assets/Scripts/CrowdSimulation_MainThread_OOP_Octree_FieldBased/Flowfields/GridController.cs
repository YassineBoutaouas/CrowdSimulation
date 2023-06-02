using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flowfield
{

    public class GridController : MonoBehaviour
    {
        public Vector2Int GridSize;

        public float CellRadius = 0.5f;

        public Flowfield CurrentFlowField;

        public bool DebugGrid;

        public void InitializeFlowfield()
        {
            CurrentFlowField = new Flowfield(CellRadius, GridSize);
            CurrentFlowField.CreateGrid(transform.position);
        }

        public void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                Gizmos.color = Color.magenta + Color.cyan;
                Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x, 2, GridSize.y));
            }

            if (!DebugGrid) return;

            Gizmos.color = Color.magenta + Color.cyan;

            float cellDiameter = CellRadius * 2;

            Vector3 offset = new Vector3(transform.position.x - GridSize.x * CellRadius, transform.position.y, transform.position.z - GridSize.y * CellRadius);

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector3 center = new Vector3((cellDiameter * x + CellRadius) + offset.x, offset.y, (cellDiameter * y + CellRadius) + offset.z);
                    Gizmos.DrawWireCube(center, Vector3.one * cellDiameter);
                }
            }
        }
    }
}