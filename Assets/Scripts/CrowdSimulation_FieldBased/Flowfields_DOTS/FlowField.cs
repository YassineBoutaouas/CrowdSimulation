using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Flowfield_DOTS
{
    public class FlowField : MonoBehaviour
    {
        public NativeArray<Cell> Grid;

        public int2 GridSize;
        public float CellRadius;

        public bool DebugGrid;

        public Transform Goal;

        private float _cellDiameter;

        public float3 GridOrigin { get; private set; }
        public Cell Destination { get; private set; }

        public readonly int2 None = new int2(0, 0);
        public readonly int2 North = new int2(0, 1);
        public readonly int2 South = new int2(0, -1);
        public readonly int2 East = new int2(1, 0);
        public readonly int2 West = new int2(-1, 0);
        public readonly int2 NorthEast = new int2(1, 1);
        public readonly int2 NorthWest = new int2(-1, 1);
        public readonly int2 SouthEast = new int2(1, -1);
        public readonly int2 SouthWest = new int2(-1, -1);

        public NativeArray<int2> CardinalDirections;
        public NativeArray<int2> CardinalAndInterCardinalDirections;
        public NativeArray<int2> AllDirections;

        private void Awake()
        {
            GridOrigin = new float3(transform.position.x - GridSize.x * CellRadius, transform.position.y, transform.position.z - GridSize.y * CellRadius);
            _cellDiameter = CellRadius * 2;

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

            CreateGrid();

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

            CreateFlowFieldJob createFlowField = new CreateFlowFieldJob(Grid, GridSize, GridOrigin, CellRadius, Goal.position, CardinalDirections, CardinalAndInterCardinalDirections, AllDirections);

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
                        Handles.Label(Grid[totalIndex].WorldPosition, Grid[totalIndex].BestCost.ToString(), style);

                        Gizmos.color = Grid[totalIndex].Cost == 255 ? Color.red : Color.white;
                    }

                    Gizmos.DrawWireCube(center, Vector3.one * _cellDiameter);
                }
            }
        }
#endif

        [BurstCompile]
        private struct GetCellFromWorldPositionJob : IJob, IDisposable
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
        private struct CreateFlowFieldJob : IJob, IDisposable
        {
            public NativeArray<Cell> Grid;
            public int2 GridSize;
            public float3 GridOrigin;

            public float CellRadius;
            public float CellDiameter;

            public float3 Destination;

            public NativeArray<int2> CardinalDirections;
            public NativeArray<int2> CardinalAndInterCardinalDirections;
            public NativeArray<int2> AllDirections;

            public CreateFlowFieldJob(NativeArray<Cell> gridCells, int2 gridSize, float3 gridOrigin, float cellRadius, float3 destination, NativeArray<int2> cardinalDirections, NativeArray<int2> cardinalAndInterCardinalDirections, NativeArray<int2> allDirections)
            {
                Grid = gridCells;
                GridSize = gridSize;
                GridOrigin = gridOrigin;
                CellRadius = cellRadius;
                CellDiameter = cellRadius * 2;
                Destination = destination;

                CardinalDirections = cardinalDirections;
                CardinalAndInterCardinalDirections = cardinalAndInterCardinalDirections;
                AllDirections = allDirections;
            }

            [BurstCompile]
            public void Execute()
            {
                CreateIntegrationField();
                //CreateFlowField();
            }

            public void CreateIntegrationField()
            {
                //for (int i = 0; i < Grid.Length; i++)
                //{
                //    Cell cell = Grid[i];
                //    cell.BestCost = ushort.MaxValue;
                //    Grid[i] = cell;
                //}

                Cell destinationCell = GetCellFromWorldPosition(Destination, out int2 destinationCellIndex);

                destinationCell.Cost = 0;
                destinationCell.BestCost = 0;

                Grid[destinationCellIndex.x.CalculateFlatIndex(destinationCellIndex.y, GridSize.x)] = destinationCell;

                NativeQueue<int2> cellsToCheck = new NativeQueue<int2>(Allocator.TempJob);

                cellsToCheck.Enqueue(destinationCellIndex);

                while (cellsToCheck.Count > 0)
                {
                    int2 currentCellIndex = cellsToCheck.Dequeue();
                    int totalIndex = currentCellIndex.x.CalculateFlatIndex(currentCellIndex.y, GridSize.x);
                    NativeList<Cell> currentNeighbors = GetNeighborCells(currentCellIndex, CardinalDirections);
                
                    for (int i = 0; i < currentNeighbors.Length; i++)
                    {
                        Cell neighbor = currentNeighbors[i];
                
                        if (neighbor.Cost == byte.MaxValue) continue;
                        if (neighbor.Cost + Grid[totalIndex].BestCost < neighbor.BestCost)
                        {
                            neighbor.BestCost = (ushort)(neighbor.Cost + Grid[totalIndex].BestCost);
                
                            currentNeighbors[i] = neighbor;
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

                    NativeList<Cell> neighbors = GetNeighborCells(cell.GridIndex, AllDirections);

                    int bestCost = cell.BestCost;

                    foreach (Cell neighbor in neighbors)
                    {
                        if (neighbor.BestCost < bestCost)
                        {
                            bestCost = neighbor.BestCost;
                            cell.BestDirection = GetDirection(neighbor.GridIndex - cell.GridIndex);
                        }
                    }

                    Grid[i] = cell;

                    neighbors.Dispose();
                }
            }

            public int2 GetDirection(int2 vector)
            {
                for (int i = 0; i < CardinalAndInterCardinalDirections.Length; i++)
                {
                    if (vector.Equals(CardinalAndInterCardinalDirections[i]))
                        return CardinalAndInterCardinalDirections[i];
                }

                return AllDirections[0];
            }

            public NativeList<Cell> GetNeighborCells(int2 gridIndex, NativeArray<int2> gridDirections)
            {
                NativeList<Cell> neighbors = new NativeList<Cell>(Allocator.Temp);

                foreach (int2 currentDir in gridDirections)
                {
                    int neighborIndex = GetCellAtRelativePos(gridIndex, currentDir);
                    if (neighborIndex != -1)
                        neighbors.Add(Grid[neighborIndex]);
                }

                return neighbors;
            }

            public int GetCellAtRelativePos(int2 gridPos, int2 offset)
            {
                int2 finalPos = gridPos + offset;

                if (finalPos.x < 0 || finalPos.x >= GridSize.x || finalPos.y < 0 || finalPos.y >= GridSize.y)
                    return -1;

                return finalPos.x.CalculateFlatIndex(finalPos.y, GridSize.x);
            }

            public Cell GetCellFromWorldPosition(float3 worldPos, out int2 index)
            {
                float percentX = (worldPos.x) / (GridSize.x * CellDiameter);
                float percentY = (worldPos.z) / (GridSize.y * CellDiameter);

                percentX = math.clamp(percentX.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);
                percentY = math.clamp(percentY.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);

                int x = math.clamp((int)math.floor(GridSize.x * percentX), 0, GridSize.x - 1);
                int y = math.clamp((int)math.floor(GridSize.y * percentY), 0, GridSize.y - 1);

                index = new int2(x, y); // x.CalculateFlatIndex(y, GridSize.x);

                return Grid[x.CalculateFlatIndex(y, GridSize.x)];
            }

            public void Dispose()
            {
                return;
            }
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
    }
}