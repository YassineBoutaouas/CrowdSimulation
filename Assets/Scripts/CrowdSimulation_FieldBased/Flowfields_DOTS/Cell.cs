using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using JetBrains.Annotations;
using System;

namespace Flowfield_DOTS
{
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

    //public struct GridDirection
    //{
    //    public int2 Vector;
    //
    //    public GridDirection(int x, int y)
    //    {
    //        Vector = new int2(x, y);
    //    }
    //
    //    //public static implicit operator int2(GridDirection direction)
    //    //{
    //    //    return direction.Vector;
    //    //}
    //
    //    public static int2 GetDirection(int2 vector)
    //    {
    //        for (int i = 0; i < GridDirectionExtension.CardinalAndInterCardinalDirections.Length; i++)
    //        {
    //            if (vector.Equals(GridDirectionExtension.CardinalAndInterCardinalDirections[i]))
    //                return GridDirectionExtension.CardinalAndInterCardinalDirections[i];
    //        }
    //
    //        return GridDirectionExtension.None;
    //    }
    //}

    //public static class GridDirectionExtension
    //{
    //    public static readonly int2 None = new int2(0, 0);
    //    public static readonly int2 North = new int2(0, 1);
    //    public static readonly int2 South = new int2(0, -1);
    //    public static readonly int2 East = new int2(1, 0);
    //    public static readonly int2 West = new int2(-1, 0);
    //    public static readonly int2 NorthEast = new int2(1, 1);
    //    public static readonly int2 NorthWest = new int2(-1, 1);
    //    public static readonly int2 SouthEast = new int2(1, -1);
    //    public static readonly int2 SouthWest = new int2(-1, -1);

    //    public static readonly NativeArray<int2> CardinalDirections = new NativeArray<int2>(4, Allocator.Persistent)
    //    {
    //        [0] = North,
    //        [1] = East,
    //        [2] = South,
    //        [3] = West
    //    };
    //    public static readonly NativeArray<int2> CardinalAndInterCardinalDirections = new NativeArray<int2>(8, Allocator.Persistent)
    //    {
    //        [0] = North,
    //        [1] = NorthEast,
    //        [2] = East,
    //        [3] = SouthEast,
    //        [4] = South,
    //        [5] = SouthWest,
    //        [6] = West,
    //        [7] = NorthWest
    //    };
    //    public static readonly NativeArray<int2> AllDirections = new NativeArray<int2>(9, Allocator.Persistent)
    //    {
    //        [0] = None,
    //        [1] = North,
    //        [2] = NorthEast,
    //        [3] = East,
    //        [4] = SouthEast,
    //        [5] = South,
    //        [6] = SouthWest,
    //        [7] = West,
    //        [8] = NorthWest
    //    };
    //}
}