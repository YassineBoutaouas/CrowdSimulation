using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
#endif

namespace Flowfield_DOTS
{
    public class FlowField : MonoBehaviour
    {
        public NativeArray<Cell> Grid;

        #region Grid properties
        public int2 GridSize;
        public float CellRadius;

        public Transform Goal;
        #endregion

        public bool DebugGrid;

        private float _cellDiameter;

        public float3 GridOrigin { get; private set; }
        //public Cell Destination { get; private set; }

        public GridDirections Directions { get; private set; }

        private ProfilerMarker _profilerMarker;

        private void Awake()
        {
            GridOrigin = new float3(transform.position.x - GridSize.x * CellRadius, transform.position.y, transform.position.z - GridSize.y * CellRadius);
            _cellDiameter = CellRadius * 2;

            Directions = new GridDirections(0);
            _profilerMarker = new ProfilerMarker("Flowfield.Create");

            CreateGrid();
        }

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            CreateFlowField();
        }

        private void CreateGrid()
        {
            if (Grid.IsCreated) Grid.Dispose();

            Grid = new NativeArray<Cell>(GridSize.x * GridSize.y, Allocator.Persistent);

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    int totalIndex = x.CalculateFlatIndex(y, GridSize.x);
                    float3 worldPos = new float3((_cellDiameter * x + CellRadius) + GridOrigin.x, GridOrigin.y, (_cellDiameter * y + CellRadius) + GridOrigin.z);

                    byte cost = 1;
                    if (!NavMesh.SamplePosition(worldPos, out NavMeshHit _, CellRadius * 1.5f, NavMesh.AllAreas))
                        cost = 255;

                    Cell cell = new Cell(worldPos, new int2(x, y));
                    cell.SetCost(cost);

                    Grid[totalIndex] = cell;
                }
            }
        }

        public void CreateFlowField()
        {
            JobHandle jobHandle;

            CreateFlowFieldJob createFlowField = new CreateFlowFieldJob(Grid, GridSize, GridOrigin, CellRadius, Goal.position, Directions, _profilerMarker);

            jobHandle = createFlowField.Schedule();

            jobHandle.Complete();

            Grid = createFlowField.Grid;

            createFlowField.Dispose();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!DebugGrid) return;

            Gizmos.color = Color.grey;
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

                    if (Grid.IsCreated && Grid.Length > 0)
                    {
                        //Handles.Label(Grid[totalIndex].WorldPosition, Grid[totalIndex].Cost.ToString(), style);
                        //Handles.Label(Grid[totalIndex].WorldPosition, Grid[totalIndex].BestCost.ToString(), style);
                        //FlowFieldExtensions.GizmosDrawArrow(Grid[totalIndex].WorldPosition, new Vector3(Grid[totalIndex].BestDirection.x, 0, Grid[totalIndex].BestDirection.y));

                        Gizmos.color = Grid[totalIndex].Cost == 255 ? Color.red : Color.white;
                    }

                    Gizmos.DrawWireCube(center, Vector3.one * _cellDiameter);
                }
            }
        }
#endif
    }

    public struct GridDirections
    {
        public int2 None;
        public int2 North;
        public int2 South;
        public int2 East;
        public int2 West;

        public int2 NorthEast;
        public int2 NorthWest;
        public int2 SouthEast;
        public int2 SouthWest;

        public NativeArray<int2> CardinalDirections;
        public NativeArray<int2> CardinalAndInterCardinalDirections;
        public NativeArray<int2> AllDirections;

        public GridDirections(int i)
        {
            None = new int2(0, 0);
            North = new int2(0, 1);
            South = new int2(0, -1);
            East = new int2(1, 0);
            West = new int2(-1, 0);

            NorthEast = new int2(1, 1);
            NorthWest = new int2(-1, 1);
            SouthEast = new int2(1, -1);
            SouthWest = new int2(-1, -1);

            CardinalDirections = new NativeArray<int2>(4, Allocator.Persistent)
            {
                [0] = North,
                [1] = East,
                [2] = South,
                [3] = West
            };
            CardinalAndInterCardinalDirections = new NativeArray<int2>(8, Allocator.Persistent)
            {
                [0] = North,
                [1] = NorthEast,
                [2] = East,
                [3] = SouthEast,
                [4] = South,
                [5] = SouthWest,
                [6] = West,
                [7] = NorthWest
            };
            AllDirections = new NativeArray<int2>(9, Allocator.Persistent)
            {
                [0] = None,
                [1] = North,
                [2] = NorthEast,
                [3] = East,
                [4] = SouthEast,
                [5] = South,
                [6] = SouthWest,
                [7] = West,
                [8] = NorthWest
            };
        }
    }

    [BurstCompile]
    public struct GetCellFromWorldPositionJob : IJob, IDisposable
    {
        public NativeArray<Cell> Grid;
        public int2 GridSize;
        public float3 GridOrigin;

        public float CellRadius;
        public float CellDiameter;

        public float3 Destination;
        public NativeArray<int> TargetDirection;

        public GetCellFromWorldPositionJob(NativeArray<Cell> gridCells, int2 gridSize, float3 gridOrigin, float cellRadius, float3 destination)
        {
            Grid = gridCells;
            GridSize = gridSize;
            GridOrigin = gridOrigin;
            CellRadius = cellRadius;
            CellDiameter = cellRadius * 2;
            Destination = destination;

            TargetDirection = new NativeArray<int>(2, Allocator.TempJob);
        }

        [BurstCompile]
        public void Execute()
        {
            Cell targetCell = GetCellFromWorldPosition(Destination, out int _);
            TargetDirection[0] = targetCell.GridIndex.x;
            TargetDirection[1] = targetCell.GridIndex.y;
        }

        public Cell GetCellFromWorldPosition(float3 worldPos, out int totalIndex)
        {
            float percentX = (worldPos.x) / (GridSize.x * CellDiameter);
            float percentY = (worldPos.z) / (GridSize.y * CellDiameter);

            percentX = math.clamp(percentX.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);
            percentY = math.clamp(percentY.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);

            int x = math.clamp((int)math.floor(GridSize.x * percentX), 0, GridSize.x - 1);
            int y = math.clamp((int)math.floor(GridSize.y * percentY), 0, GridSize.y - 1);

            totalIndex = x.CalculateFlatIndex(y, GridSize.x);

            return Grid[totalIndex];
        }

        public void Dispose()
        {
            return;
        }
    }

    [BurstCompile]
    public struct CreateFlowFieldJob : IJob, IDisposable
    {
        public NativeArray<Cell> Grid;
        public int2 GridSize;
        public float3 GridOrigin;

        public float CellRadius;
        public float CellDiameter;

        public float3 Destination;

        public GridDirections Directions;

        public int2 InvalidDirection;

        public ProfilerMarker profilerMarker;

        public CreateFlowFieldJob(NativeArray<Cell> gridCells, int2 gridSize, float3 gridOrigin, float cellRadius, float3 destination, GridDirections directions, ProfilerMarker marker)
        {
            Grid = gridCells;
            GridSize = gridSize;
            GridOrigin = gridOrigin;
            CellRadius = cellRadius;
            CellDiameter = cellRadius * 2;
            Destination = destination;

            Directions = directions;
            InvalidDirection = new int2(-1, -1);

            profilerMarker = marker;
        }

        [BurstCompile]
        public void Execute()
        {
            profilerMarker.Begin();
            CreateIntegrationField();
            CreateFlowField();
            profilerMarker.End();
        }

        public void CreateIntegrationField()
        {

            Cell destinationCell = GetCellFromWorldPosition(Destination, out int2 destinationCellIndex);

            destinationCell.Cost = 0;
            destinationCell.BestCost = 0;

            Grid[destinationCellIndex.x.CalculateFlatIndex(destinationCellIndex.y, GridSize.x)] = destinationCell;

            NativeQueue<int2> cellsToCheck = new NativeQueue<int2>(Allocator.TempJob);

            cellsToCheck.Enqueue(destinationCellIndex);

            while (cellsToCheck.Count > 0)
            {
                int2 currentCellIndex = cellsToCheck.Dequeue();
                int currentCelltotalIndex = currentCellIndex.x.CalculateFlatIndex(currentCellIndex.y, GridSize.x);

                NativeList<Cell> currentNeighbors = GetNeighborCells(currentCellIndex, Directions.CardinalDirections);

                for (int i = 0; i < currentNeighbors.Length; i++)
                {
                    Cell currNeighbor = currentNeighbors[i];

                    if (currNeighbor.Cost == byte.MaxValue) continue;
                    if (currNeighbor.Cost + Grid[currentCelltotalIndex].BestCost < currNeighbor.BestCost)
                    {
                        currNeighbor.BestCost = (ushort)(currNeighbor.Cost + Grid[currentCelltotalIndex].BestCost);

                        currentNeighbors[i] = currNeighbor;
                        Grid[currNeighbor.GridIndex.x.CalculateFlatIndex(currNeighbor.GridIndex.y, GridSize.x)] = currNeighbor;

                        cellsToCheck.Enqueue(currentNeighbors[i].GridIndex);
                    }
                }

                currentNeighbors.Dispose();
            }

            cellsToCheck.Dispose();
        }

        public void CreateFlowField()
        {
            for (int i = 0; i < Grid.Length; i++)
            {
                Cell cell = Grid[i];

                NativeList<Cell> neighbors = GetNeighborCells(cell.GridIndex, Directions.AllDirections);

                int bestCost = cell.BestCost;
                int2 lowestCostIndex = cell.GridIndex;

                //get lowest cost cell - could be done through priority queue
                foreach (Cell neighbor in neighbors)
                {
                    if (neighbor.BestCost < bestCost)
                    {
                        bestCost = neighbor.BestCost;
                        lowestCostIndex = neighbor.GridIndex;
                    }
                }

                cell.BestDirection = GetDirection(lowestCostIndex - cell.GridIndex); //only has to be calculated once really

                Grid[i] = cell;

                neighbors.Dispose();
            }
        }

        public int2 GetDirection(int2 vector)
        {
            for (int i = 0; i < Directions.CardinalAndInterCardinalDirections.Length; i++)
            {
                if (vector.Equals(Directions.CardinalAndInterCardinalDirections[i]))
                    return Directions.CardinalAndInterCardinalDirections[i];
            }

            return Directions.AllDirections[0];
        }

        public NativeList<Cell> GetNeighborCells(int2 gridIndex, NativeArray<int2> gridDirections)
        {
            NativeList<Cell> neighbors = new NativeList<Cell>(Allocator.Temp);

            foreach (int2 currentDir in gridDirections)
            {
                int2 neighborIndex = GetCellAtRelativePos(gridIndex, currentDir);

                if (!neighborIndex.Equals(InvalidDirection))
                    neighbors.Add(Grid[neighborIndex.x.CalculateFlatIndex(neighborIndex.y, GridSize.x)]);
            }

            return neighbors;
        }

        public int2 GetCellAtRelativePos(int2 gridPos, int2 offset)
        {
            int2 finalPos = gridPos + offset;

            if (finalPos.x < 0 || finalPos.x >= GridSize.x || finalPos.y < 0 || finalPos.y >= GridSize.y)
                return InvalidDirection;

            return finalPos;
        }

        public Cell GetCellFromWorldPosition(float3 worldPos, out int2 index)
        {
            float percentX = (worldPos.x) / (GridSize.x * CellDiameter);
            float percentY = (worldPos.z) / (GridSize.y * CellDiameter);

            percentX = math.clamp(percentX.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);
            percentY = math.clamp(percentY.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);

            int x = math.clamp((int)math.floor(GridSize.x * percentX), 0, GridSize.x - 1);
            int y = math.clamp((int)math.floor(GridSize.y * percentY), 0, GridSize.y - 1);

            index = new int2(x, y);

            return Grid[x.CalculateFlatIndex(y, GridSize.x)];
        }

        public void Dispose()
        {
            return;
        }
    }

    public static class FlowFieldExtensions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static int CalculateFlatIndex(this int itemIndex, int listIndex, int gridWidth)
        {
            return itemIndex + listIndex * gridWidth;
        }

        public static void GizmosDrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            Gizmos.DrawRay(pos + Vector3.up + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + Vector3.up + direction, left * arrowHeadLength);
        }
    }
}