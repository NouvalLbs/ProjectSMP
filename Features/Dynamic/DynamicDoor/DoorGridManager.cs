using System;
using System.Collections.Generic;

namespace ProjectSMP.Features.Dynamic.DynamicDoor
{
    internal static class DoorGridManager
    {
        private const float GridSize = 100.0f;
        private const int GridCells = 100;

        private static readonly List<int>[,] Grid = new List<int>[GridCells, GridCells];

        public static void Initialize()
        {
            for (int x = 0; x < GridCells; x++)
            {
                for (int y = 0; y < GridCells; y++)
                {
                    Grid[x, y] = new List<int>();
                }
            }
        }

        public static void AddDoor(int doorId, float posX, float posY)
        {
            GetGridCell(posX, posY, out int cellX, out int cellY);
            if (!Grid[cellX, cellY].Contains(doorId))
            {
                Grid[cellX, cellY].Add(doorId);
            }
        }

        public static void RemoveDoor(int doorId, float posX, float posY)
        {
            GetGridCell(posX, posY, out int cellX, out int cellY);
            Grid[cellX, cellY].Remove(doorId);
        }

        public static List<int> GetDoorsInCell(float posX, float posY)
        {
            GetGridCell(posX, posY, out int cellX, out int cellY);
            return new List<int>(Grid[cellX, cellY]);
        }

        private static void GetGridCell(float x, float y, out int cellX, out int cellY)
        {
            cellX = (int)Math.Floor(x / GridSize);
            cellY = (int)Math.Floor(y / GridSize);

            cellX = Math.Clamp(cellX, 0, GridCells - 1);
            cellY = Math.Clamp(cellY, 0, GridCells - 1);
        }
    }
}