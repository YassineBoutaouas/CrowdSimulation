using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flowfield
{

    public class Cell
    {
        public Vector3 WorldPosition;
        public Vector2Int GridPosition;

        public Cell(Vector3 worldPos, Vector2Int gridPos)
        {
            WorldPosition = worldPos;
            GridPosition = gridPos;
        }
    }
}