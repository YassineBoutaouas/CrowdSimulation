using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Flowfield
{
    /// <summary>
    /// Creates, references and visualizes a flowfield
    /// </summary>
    public class GridController : MonoBehaviour
    {
        public Vector2Int GridSize;
        public float CellRadius = 0.5f;
        public Flowfield CurrentFlowField;

        public Transform Goal;

        public bool DebugGrid;
        public enum FlowFieldDisplay { None, Cost, Integration, Flow }
        public FlowFieldDisplay FlowFieldDisplayType;

        public LayerMask Layer;

        private void Start()
        {
            CurrentFlowField = new Flowfield(CellRadius, GridSize, transform.position, Layer);
            CurrentFlowField.CreateGrid(transform.position);
            CurrentFlowField.CreateCostField(); //Assumes that the map is static

            CurrentFlowField.profilerMarker.Begin();
            CurrentFlowField.SetDestination(Goal.position);
            CurrentFlowField.CreateFlowField(); //May be created whenever the goal position has changed
            CurrentFlowField.profilerMarker.End();
        }

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
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
                            case FlowFieldDisplay.Flow:
                                Vector3 dir = new Vector3(CurrentFlowField.Grid[x, y].BestDirection.Vector.x, 0, CurrentFlowField.Grid[x, y].BestDirection.Vector.y);
                                FlowfieldExtensions.GizmosDrawArrow(CurrentFlowField.Grid[x, y].WorldPosition, dir);
                                break;
                            default:
                                break;
                        }

                        Gizmos.color = CurrentFlowField.Grid[x, y].Cost == 255 ? Color.red : Color.white;
                    }

                    Gizmos.DrawWireCube(center, Vector3.one * cellDiameter);
                }
            }
        }
#endif
    }
}