using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Flowfield
{

    public class GridController : MonoBehaviour
    {
        public Vector2Int GridSize;
        public float CellRadius = 0.5f;
        public Flowfield CurrentFlowField;

        public Transform Goal;

        public bool DebugGrid;
        public enum FlowFieldDisplay { None, Cost, Integration }
        public FlowFieldDisplay FlowFieldDisplayType;
        private Color _gizmoColor = Color.green + Color.grey;

        private Vector3 lastMouse;

        private void Start()
        {
            InitializeFlowfield();
        }

        public void InitializeFlowfield()
        {
            CurrentFlowField = new Flowfield(CellRadius, GridSize, transform.position);
            CurrentFlowField.CreateGrid(transform.position);
            CurrentFlowField.CreateCostField();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                CurrentFlowField.SetDestination(Goal.position);
            }
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x, 2, GridSize.y));
            }

            if (!DebugGrid) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            float cellDiameter = CellRadius * 2;

            Vector3 offset = new Vector3(transform.position.x - GridSize.x * CellRadius, transform.position.y, transform.position.z - GridSize.y * CellRadius);

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector3 center = new Vector3((cellDiameter * x + CellRadius) + offset.x, offset.y, (cellDiameter * y + CellRadius) + offset.z);
            
                    if (CurrentFlowField != null)
                    {
                        switch (FlowFieldDisplayType)
                        {
                            case FlowFieldDisplay.None:
                                break;
                            case FlowFieldDisplay.Cost:
                                Handles.Label(CurrentFlowField.Grid[x, y].WorldPosition, CurrentFlowField.Grid[x, y].Cost.ToString(), style);
                                break;
                            case FlowFieldDisplay.Integration:
                                Handles.Label(CurrentFlowField.Grid[x, y].WorldPosition, CurrentFlowField.Grid[x, y].BestCost.ToString(), style);
                                break;
                            default:
                                break;
                        }
            
                        Gizmos.color = CurrentFlowField.Grid[x, y].Cost == 255 ? Color.red : Color.white;
                    }
            
                    Gizmos.DrawWireCube(center, Vector3.one * cellDiameter);
                }
            }

            Gizmos.color = Color.blue;
            if (CurrentFlowField != null)
                if (CurrentFlowField.Destination != null)
                    Gizmos.DrawSphere(CurrentFlowField.Destination.WorldPosition, CurrentFlowField.CellRadius);
        }
#endif
    }
}