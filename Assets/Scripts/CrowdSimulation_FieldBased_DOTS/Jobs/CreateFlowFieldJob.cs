using Global;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace Flowfield_DOTS
{
    [BurstCompile]
    public struct GetCellFromWorldPositionJob : IJob, IDisposable
    {
        #region Properties
        public NativeArray<Cell> Grid;
        public int2 GridSize;
        public float3 GridOrigin;

        public float CellRadius;
        public float CellDiameter;

        public float3 Destination;
        public NativeArray<int> TargetDirection;
        #endregion

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
        #region Properties
        public NativeArray<Cell> Grid;
        public int2 GridSize;
        public float3 GridOrigin;

        public float CellRadius;
        public float CellDiameter;

        public float3 Destination;

        public GridDirections Directions;

        public int2 InvalidDirection;
        #endregion

        public CreateFlowFieldJob(NativeArray<Cell> gridCells, int2 gridSize, float3 gridOrigin, float cellRadius, float3 destination, GridDirections directions)
        {
            Grid = gridCells;
            GridSize = gridSize;
            GridOrigin = gridOrigin;
            CellRadius = cellRadius;
            CellDiameter = cellRadius * 2;
            Destination = destination;

            Directions = directions;
            InvalidDirection = new int2(-1, -1);
        }

        [BurstCompile]
        public void Execute()
        {
            CreateIntegrationField();
            CreateFlowField();
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
}