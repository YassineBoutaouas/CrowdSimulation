using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace Flowfield
{
    /// <summary>
    /// Contains methods to create a flowfield
    /// </summary>
    public class Flowfield
    {
        public Cell[,] Grid { get; private set; }
        public Vector2Int GridSize { get; private set; }
        public readonly Vector3 GridOrigin;

        public float CellRadius { get; private set; }
        public float CellDiameter { get; private set; }

        public Cell Destination;

        public ProfilerMarker profilerMarker = new ProfilerMarker("Flowfield.Create");

        public LayerMask Layer;

        public Flowfield(float cellRadius, Vector2Int gridSize, Vector3 worldOrigin, LayerMask layer)
        {
            CellRadius = cellRadius;
            CellDiameter = cellRadius * 2f;
            GridSize = gridSize;

            Layer = layer;

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

        /// <summary>
        /// Creates an initial cost field based on physics layer overlap tests
        /// </summary>
        public void CreateCostField()
        {
            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector3 worldPos = Grid[x, y].WorldPosition;

                    byte cost = 1;
                    if (Physics.OverlapBox(worldPos, CellRadius * Vector3.one, Quaternion.identity, Layer).Length > 0)
                    {
                        cost = 255;
                    }

                    Grid[x, y].SetCost(cost);
                }
            }
        }

        /// <summary>
        /// Creates an integration field based on the given cost field
        /// </summary>
        public void CreateIntegrationField(Cell destination)
        {
            foreach (Cell c in Grid)
            {
                c.BestCost = ushort.MaxValue;
            }

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
                    if (currNeighbor.Cost + currCell.BestCost < currNeighbor.BestCost)
                    {
                        currNeighbor.BestCost = (ushort)(currNeighbor.Cost + currCell.BestCost);
                        cellsToCheck.Enqueue(currNeighbor);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a flow field based on the given integration field
        /// </summary>
        public void CreateFlowField()
        {
            foreach (Cell c in Grid)
            {
                List<Cell> neighbors = GetNeighborCells(c.GridIndex, GridDirection.AllDirections);

                int bestCost = c.BestCost;

                foreach (Cell n in neighbors)
                {
                    if (n.BestCost < bestCost)
                    {
                        bestCost = n.BestCost;
                        c.BestDirection = GridDirection.GetDirection(n.GridIndex - c.GridIndex);
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

        /// <summary>
        /// Returns the cells in each given cardinal direction
        /// </summary>
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

        /// <summary>
        /// Retrieves a cell at a relative position
        /// </summary>
        private Cell GetCellAtRelativePos(Vector2Int originPos, Vector2Int relativePos)
        {
            Vector2Int finalPos = originPos + relativePos;
            if (finalPos.x < 0 || finalPos.x >= GridSize.x || finalPos.y < 0 || finalPos.y >= GridSize.y)
                return null;

            return Grid[finalPos.x, finalPos.y];
        }

        /// <summary>
        /// Retrieves a cell from an absolute world position
        /// </summary>
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

    /// <summary>
    /// Contains global extension methods
    /// </summary>
    public static class FlowfieldExtensions
    {
        public static float Remap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static void DebugDrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Debug.DrawRay(pos, direction);

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Debug.DrawRay(pos + direction, right * arrowHeadLength);
            Debug.DrawRay(pos + direction, left * arrowHeadLength);
        }

        public static void GizmosDrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            Gizmos.DrawRay(pos, direction);

            if (direction.sqrMagnitude == 0) return;

            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + Vector3.up + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + Vector3.up + direction, left * arrowHeadLength);
        }
    }
}