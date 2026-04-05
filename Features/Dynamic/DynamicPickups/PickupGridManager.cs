using System;
using System.Collections.Generic;

namespace ProjectSMP.Features.Dynamic.DynamicPickups
{
    internal static class PickupGridManager
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

        public static void AddPickup(int pickupId, float posX, float posY)
        {
            GetGridCell(posX, posY, out int cellX, out int cellY);
            if (!Grid[cellX, cellY].Contains(pickupId))
            {
                Grid[cellX, cellY].Add(pickupId);
            }
        }

        public static void RemovePickup(int pickupId, float posX, float posY)
        {
            GetGridCell(posX, posY, out int cellX, out int cellY);
            Grid[cellX, cellY].Remove(pickupId);
        }

        public static List<int> GetPickupsInCell(float posX, float posY)
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