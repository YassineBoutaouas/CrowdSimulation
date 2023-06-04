using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Flowfield
{

    public class Flowfield
    {
        public Cell[,] Grid { get; private set; }
        public Vector2Int GridSize { get; private set; }
        public readonly Vector3 GridOrigin;

        public float CellRadius { get; private set; }
        public float CellDiameter { get; private set; }

        public Cell Destination;

        public Flowfield(float cellRadius, Vector2Int gridSize, Vector3 worldOrigin)
        {
            CellRadius = cellRadius;
            CellDiameter = cellRadius * 2f;
            GridSize = gridSize;

            GridOrigin = new Vector3(worldOrigin.x - GridSize.x * CellRadius, worldOrigin.y, worldOrigin.z - GridSize.y * CellRadius);
        }

        public void CreateGrid(Vector3 posoffset)
        {
            Grid = new Cell[GridSize.x, GridSize.y];

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector3 worldPos = new Vector3((CellDiameter * x + CellRadius) + GridOrigin.x, GridOrigin.y, (CellDiameter * y + CellRadius) + GridOrigin.z);
                    Grid[x, y] = new Cell(worldPos, new Vector2Int(x, y));
                }
            }
        }

        public void CreateCostField()
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    if (!NavMesh.SamplePosition(Grid[x, y].WorldPosition, out NavMeshHit _, CellRadius * 1.5f, NavMesh.AllAreas))
                        Grid[x, y].IncreaseCost(255);
                }
            }
        }

        public void CreateIntegrationField(Cell destination)
        {
            Destination = destination;

            Destination.Cost = 0;
            Destination.BestCost = 0;

            Queue<Cell> cellsToCheck = new Queue<Cell>();

            cellsToCheck.Enqueue(Destination);

            while (cellsToCheck.Count > 0)
            {
                Cell currCell = cellsToCheck.Dequeue();
                List<Cell> currNeighbors = GetNeighborCells(currCell.GridIndex, GridDirection.CardinalDirections);
                foreach (Cell currNeighbor in currNeighbors)
                {
                    if (currNeighbor.Cost == byte.MaxValue) continue;
                    if (currNeighbor.Cost + currCell.BestCost < currNeighbor.BestCost) //accumulative cost
                    {
                        currNeighbor.BestCost = (ushort)(currNeighbor.Cost + currCell.BestCost);
                        cellsToCheck.Enqueue(currNeighbor);
                    }
                }
            }
        }

        public void SetDestination(Vector3 position)
        {
            Vector3 pos = new Vector3(position.x, position.y, position.z);
            Cell destinationCell = GetCellFromWorldPosition(pos);
            CreateIntegrationField(destinationCell);
        }

        private List<Cell> GetNeighborCells(Vector2Int gridIndex, List<GridDirection> directions)
        {
            List<Cell> neighbors = new List<Cell>();

            foreach (Vector2Int currentDirection in directions)
            {
                Cell newNeighbor = GetCellAtRelativePos(gridIndex, currentDirection);
                if (newNeighbor != null)
                    neighbors.Add(newNeighbor);
            }

            return neighbors;
        }

        private Cell GetCellAtRelativePos(Vector2Int originPos, Vector2Int relativePos)
        {
            Vector2Int finalPos = originPos + relativePos;
            if (finalPos.x < 0 || finalPos.x >= GridSize.x || finalPos.y < 0 || finalPos.y >= GridSize.y)
                return null;

            return Grid[finalPos.x, finalPos.y];
        }

        public Cell GetCellFromWorldPosition(Vector3 worldPos)
        {
            float percentX = (worldPos.x) / (GridSize.x * CellDiameter);
            float percentY = (worldPos.z) / (GridSize.y * CellDiameter);

            percentX = percentX.Remap(-0.5f, 0.5f, 0f, 1f);
            percentY = percentY.Remap(-0.5f, 0.5f, 0f, 1f);

            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            int x = Mathf.Clamp(Mathf.FloorToInt((GridSize.x) * percentX), 0, GridSize.x - 1);
            int y = Mathf.Clamp(Mathf.FloorToInt((GridSize.y) * percentY), 0, GridSize.y - 1);
            return Grid[x, y];
        }
    }

    public static class FlowfieldExtensions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }
}