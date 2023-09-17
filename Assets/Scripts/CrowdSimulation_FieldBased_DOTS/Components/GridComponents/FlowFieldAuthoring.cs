using Global;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
using Unity.AI.Navigation;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Flowfield_DOTS
{
    /// <summary>
    /// Wrapper class containing methods to initialize an unmanaged flowfield (Mirror object for debugging)
    /// </summary>
    public class FlowFieldAuthoring : MonoBehaviour
    {
        #region Grid properties
        public int2 GridSize;
        public float CellRadius;

        public Transform Goal;

        public bool DebugGrid;

        private float _cellDiameter;

        public float3 GridOrigin { get; private set; }
        #endregion

        public enum FlowFieldDebug { Cost, Integration, Flow }
        public FlowFieldDebug DebugMethod = FlowFieldDebug.Cost;

        private FlowFieldComponent _flowField;

        private void Awake()
        {
            GridOrigin = new float3(transform.position.x - GridSize.x * CellRadius, transform.position.y, transform.position.z - GridSize.y * CellRadius);
            _cellDiameter = CellRadius * 2;

            NativeArray<Cell> Grid = CreateGrid();

            //Creates a localtransform component as copy of the goal - modifications have to be done in the unmanaged parts and jobs
            LocalTransform localTransform = new LocalTransform();
            localTransform = localTransform.Translate(Goal.position);

            //Creates an entity based on the values set by this class - modifications have to be done in the unmanaged parts and jobs
            _flowField = new FlowFieldComponent(Grid, GridSize, CellRadius, GridOrigin, localTransform); //localTransform is passed as a copy

            Entity entity = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntity();
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(entity, _flowField);

            //Component may be destroyed, is kept for debugging
        }

        /// <summary>
        /// Uses the default Physics API to configure walkable areas that can then be passed to an unmanaged flowfield component.
        /// Asumes that the flowfield is static without moving environment objects
        /// </summary>
        /// <returns></returns>
        private NativeArray<Cell> CreateGrid()
        {
            NativeArray<Cell> result = new NativeArray<Cell>(GridSize.x * GridSize.y, Allocator.Persistent);

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    int totalIndex = x.CalculateFlatIndex(y, GridSize.x);
                    float3 worldPos = new float3((_cellDiameter * x + CellRadius) + GridOrigin.x, GridOrigin.y, (_cellDiameter * y + CellRadius) + GridOrigin.z);

                    byte cost = 1;

                    foreach(Collider collider in Physics.OverlapBox(worldPos, CellRadius * Vector3.one))
                    {
                        if (!collider.TryGetComponent(out NavMeshSurface _))
                        {
                            cost = 255;
                            break;
                        }
                    }

                    Cell cell = new Cell(worldPos, new int2(x, y));
                    cell.SetCost(cost);

                    result[totalIndex] = cell;
                }
            }

            return result;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Debugs the given flowfield from the managed part of Unity
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!DebugGrid) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(GridSize.x * CellRadius * 2, 10f, GridSize.y * CellRadius * 2));

            Vector3 offset = new Vector3(transform.position.x - GridSize.x * CellRadius, transform.position.y, transform.position.z - GridSize.y * CellRadius);

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector3 center = new Vector3((_cellDiameter * x + CellRadius) + offset.x, offset.y, (_cellDiameter * y + CellRadius) + offset.z);
                    int totalIndex = x.CalculateFlatIndex(y, GridSize.x);

                    if (_flowField.Grid.IsCreated && _flowField.Grid.Length > 0)
                    {
                        switch (DebugMethod)
                        {
                            case FlowFieldDebug.Cost:
                                Handles.Label(_flowField.Grid[totalIndex].WorldPosition, _flowField.Grid[totalIndex].Cost.ToString(), style);

                                break;
                            case FlowFieldDebug.Integration:
                                Handles.Label(_flowField.Grid[totalIndex].WorldPosition, _flowField.Grid[totalIndex].BestCost.ToString(), style);

                                break;
                            case FlowFieldDebug.Flow:
                                ExtensionMethods.GizmosDrawArrow(_flowField.Grid[totalIndex].WorldPosition, new Vector3(_flowField.Grid[totalIndex].BestDirection.x, 0, _flowField.Grid[totalIndex].BestDirection.y), Color.green);

                                break;
                            default:
                                break;
                        }

                        Gizmos.color = _flowField.Grid[totalIndex].Cost == 255 ? Color.red : Color.white;
                    }

                    Gizmos.DrawWireCube(center, Vector3.one * _cellDiameter);
                }
            }
        }
#endif
    }

    /// <summary>
    /// DOTS component containing a flowfield
    /// </summary>
    public struct FlowFieldComponent : IComponentData
    {
        public NativeArray<Cell> Grid;

        #region Grid properties
        public int2 GridSize;
        public float CellRadius;

        public LocalTransform Goal;
        public float3 _previousPosition;
        #endregion

        public float CellDiameter { get; private set; }

        public float3 GridOrigin { get; private set; }

        public GridDirections Directions { get; private set; }

        public FlowFieldComponent(NativeArray<Cell> grid, int2 gridSize, float cellRadius, float3 gridOrigin, LocalTransform goal)
        {
            GridSize = gridSize;
            Grid = grid;

            CellRadius = cellRadius;
            CellDiameter = cellRadius * 2;
            GridOrigin = gridOrigin;

            Goal = goal;
            _previousPosition = float3.zero;

            Directions = new GridDirections(0);
        }
    }
}