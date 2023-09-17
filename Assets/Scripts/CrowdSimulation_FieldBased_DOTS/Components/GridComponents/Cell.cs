using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Flowfield_DOTS
{
    /// <summary>
    /// Represents a cell in a flowfield, contains direction and cost values
    /// </summary>
    public struct Cell : IEquatable<Cell>
    {
        public float3 WorldPosition;
        public int2 GridIndex;

        public byte Cost;
        public ushort BestCost;

        public int2 BestDirection;

        public bool Initialized;

        public Cell(float3 worldPos, int2 gridIndex)
        {
            WorldPosition = worldPos;
            GridIndex = gridIndex;
            Cost = 1;
            BestCost = ushort.MaxValue;
            BestDirection = default;

            Initialized = true;
        }

        public bool Equals(Cell other)
        {
            return WorldPosition.Equals(other.WorldPosition);
        }

        public bool IsNull()
        {
            return Initialized;
        }

        public void SetCost(int cost)
        {
            Cost = (byte)math.clamp(cost, 1, 255);
        }
    }

    /// <summary>
    /// Helper class declaring cardinal directions to be used in a flowfield cell
    /// </summary>
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
}