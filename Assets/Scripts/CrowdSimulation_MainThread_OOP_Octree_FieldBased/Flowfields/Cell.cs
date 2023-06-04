using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flowfield
{
    public class Cell
    {
        public Vector3 WorldPosition;
        public Vector2Int GridIndex;

        public byte Cost;
        public ushort BestCost;

        public GridDirection BestDirection;

        public Cell(Vector3 worldPos, Vector2Int gridPos)
        {
            WorldPosition = worldPos;
            GridIndex = gridPos;
            Cost = 1;
            BestCost = ushort.MaxValue;
            BestDirection = GridDirection.None;
        }

        public void SetCost(int amount)
        {
            if(Cost == byte.MaxValue) return;

            Cost = (byte)Mathf.Clamp(amount, 1, 255);
        }
    }
}