using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flowfield
{

    public class Flowfield
    {
        public Cell[,] Grid { get; private set; }
        public Vector2Int GridSize { get; private set; }

        public float CellRadius { get; private set; }
        public float CellDiameter { get; private set; }

        public Flowfield(float cellRadius, Vector2Int gridSize)
        {
            CellRadius = cellRadius;
            CellDiameter = cellRadius * 2f;
            GridSize = gridSize;
        }

        public void CreateGrid(Vector3 posoffset)
        {
            Vector3 offset = new Vector3(posoffset.x - GridSize.x * CellRadius, posoffset.y, posoffset.z - GridSize.y * CellRadius);

            Grid = new Cell[GridSize.x, GridSize.y];

            for (int x = 0; x < GridSize.x; x++)
            {
                for (int y = 0; y < GridSize.y; y++)
                {
                    Vector3 worldPos = new Vector3((CellDiameter * x + CellRadius) + offset.x, offset.y, (CellDiameter * y + CellRadius) + offset.z);
                    Grid[x,y] = new Cell(worldPos, new Vector2Int(x,y));
                }
            }
        }
    }
}