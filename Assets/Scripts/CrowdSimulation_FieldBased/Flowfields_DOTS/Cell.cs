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
}