using Global;
using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Flowfield_DOTS
{
    /// <summary>
    /// Gets the best direction towards the target stored by a given cell within a flowfield
    /// </summary>
    [BurstCompile]
    public partial struct GetDirectionToTargetJob : IJobEntity, IDisposable
    {
        public FlowFieldComponent _flowField;

        public GetDirectionToTargetJob(FlowFieldComponent flowField)
        {
            _flowField = flowField;
            __TypeHandle = default;
        }

        [BurstCompile]
        public void Execute(FlockAgentAspect flockAgent)
        {
            float2 bestDirection = GetCellFromWorldPosition(flockAgent.Transform.ValueRW.Position);

            flockAgent.FlockAgent.ValueRW.CurrentDirection = bestDirection;
        }

        public float2 GetCellFromWorldPosition(float3 worldPos)
        {
            float percentX = (worldPos.x) / (_flowField.GridSize.x * _flowField.CellDiameter);
            float percentY = (worldPos.z) / (_flowField.GridSize.y * _flowField.CellDiameter);

            percentX = math.clamp(percentX.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);
            percentY = math.clamp(percentY.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);

            int x = math.clamp((int)math.floor(_flowField.GridSize.x * percentX), 0, _flowField.GridSize.x - 1);
            int y = math.clamp((int)math.floor(_flowField.GridSize.y * percentY), 0, _flowField.GridSize.y - 1);

            return _flowField.Grid[x.CalculateFlatIndex(y, _flowField.GridSize.x)].BestDirection;
        }

        public void Dispose() { return; }
    }

    /// <summary>
    /// Creates a flowfield based on the given parameters
    /// </summary>
    [BurstCompile]
    public struct CreateFlowFieldJob : IJob
    {
        #region Properties
        public FlowFieldComponent _flowField;

        public int2 InvalidDirection;
        #endregion

        public CreateFlowFieldJob(FlowFieldComponent flowField)
        {
            _flowField = flowField;

            InvalidDirection = new int2(-1, -1);
        }

        [BurstCompile]
        public void Execute()
        {
            CreateIntegrationField();
            CreateFlowField();
        }

        /// <summary>
        /// Creates an integration field based on the cost field created by the managed FlowFieldAuthoring component
        /// </summary>
        public void CreateIntegrationField()
        {

            Cell destinationCell = GetCellFromWorldPosition(_flowField.Goal.Position, out int2 destinationCellIndex);

            destinationCell.Cost = 0;
            destinationCell.BestCost = 0;

            _flowField.Grid[destinationCellIndex.x.CalculateFlatIndex(destinationCellIndex.y, _flowField.GridSize.x)] = destinationCell;

            NativeQueue<int2> cellsToCheck = new NativeQueue<int2>(Allocator.TempJob);

            cellsToCheck.Enqueue(destinationCellIndex);

            while (cellsToCheck.Count > 0)
            {
                int2 currentCellIndex = cellsToCheck.Dequeue();
                int currentCelltotalIndex = currentCellIndex.x.CalculateFlatIndex(currentCellIndex.y, _flowField.GridSize.x);

                NativeList<Cell> currentNeighbors = GetNeighborCells(currentCellIndex, _flowField.Directions.CardinalDirections);

                for (int i = 0; i < currentNeighbors.Length; i++)
                {
                    Cell currNeighbor = currentNeighbors[i];

                    if (currNeighbor.Cost == byte.MaxValue) continue;
                    if (currNeighbor.Cost + _flowField.Grid[currentCelltotalIndex].BestCost < currNeighbor.BestCost)
                    {
                        currNeighbor.BestCost = (ushort)(currNeighbor.Cost + _flowField.Grid[currentCelltotalIndex].BestCost);

                        currentNeighbors[i] = currNeighbor;
                        _flowField.Grid[currNeighbor.GridIndex.x.CalculateFlatIndex(currNeighbor.GridIndex.y, _flowField.GridSize.x)] = currNeighbor;

                        cellsToCheck.Enqueue(currentNeighbors[i].GridIndex);
                    }
                }

                currentNeighbors.Dispose();
            }

            cellsToCheck.Dispose();
        }

        /// <summary>
        /// Creates a flowfield based on the integration field
        /// </summary>
        public void CreateFlowField()
        {
            for (int i = 0; i < _flowField.Grid.Length; i++)
            {
                Cell cell = _flowField.Grid[i];

                NativeList<Cell> neighbors = GetNeighborCells(cell.GridIndex, _flowField.Directions.AllDirections);

                int bestCost = cell.BestCost;
                int2 lowestCostIndex = cell.GridIndex;

                //get lowest cost neighbor cell - could be done through priority queue
                foreach (Cell neighbor in neighbors)
                {
                    if (neighbor.BestCost < bestCost)
                    {
                        bestCost = neighbor.BestCost;
                        lowestCostIndex = neighbor.GridIndex;
                    }
                }

                cell.BestDirection = GetDirection(lowestCostIndex - cell.GridIndex);

                _flowField.Grid[i] = cell;

                neighbors.Dispose();
            }
        }

        /// <summary>
        /// Returns the cardinal and intercardinal direction stored by a cell
        /// </summary>
        /// <returns></returns>
        public int2 GetDirection(int2 vector)
        {
            for (int i = 0; i < _flowField.Directions.CardinalAndInterCardinalDirections.Length; i++)
            {
                if (vector.Equals(_flowField.Directions.CardinalAndInterCardinalDirections[i]))
                    return _flowField.Directions.CardinalAndInterCardinalDirections[i];
            }

            return _flowField.Directions.AllDirections[0];
        }

        /// <summary>
        /// Returns the neighboring cells
        /// </summary>
        /// <returns></returns>
        public NativeList<Cell> GetNeighborCells(int2 gridIndex, NativeArray<int2> gridDirections)
        {
            NativeList<Cell> neighbors = new NativeList<Cell>(Allocator.Temp);

            foreach (int2 currentDir in gridDirections)
            {
                int2 neighborIndex = GetCellAtRelativePos(gridIndex, currentDir);

                if (!neighborIndex.Equals(InvalidDirection))
                    neighbors.Add(_flowField.Grid[neighborIndex.x.CalculateFlatIndex(neighborIndex.y, _flowField.GridSize.x)]);
            }

            return neighbors;
        }

        /// <summary>
        /// Returns a cell at a relative position on the grid
        /// </summary>
        /// <returns></returns>
        public int2 GetCellAtRelativePos(int2 gridPos, int2 offset)
        {
            int2 finalPos = gridPos + offset;

            if (finalPos.x < 0 || finalPos.x >= _flowField.GridSize.x || finalPos.y < 0 || finalPos.y >= _flowField.GridSize.y)
                return InvalidDirection;

            return finalPos;
        }

        /// <summary>
        /// Returns a cell at an absolute world position
        /// </summary>
        /// <returns></returns>
        public Cell GetCellFromWorldPosition(float3 worldPos, out int2 index)
        {
            float percentX = (worldPos.x) / (_flowField.GridSize.x * _flowField.CellDiameter);
            float percentY = (worldPos.z) / (_flowField.GridSize.y * _flowField.CellDiameter);

            percentX = math.clamp(percentX.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);
            percentY = math.clamp(percentY.Remap(-0.5f, 0.5f, 0f, 1f), 0, 1);

            int x = math.clamp((int)math.floor(_flowField.GridSize.x * percentX), 0, _flowField.GridSize.x - 1);
            int y = math.clamp((int)math.floor(_flowField.GridSize.y * percentY), 0, _flowField.GridSize.y - 1);

            index = new int2(x, y);

            return _flowField.Grid[x.CalculateFlatIndex(y, _flowField.GridSize.x)];
        }
    }
}