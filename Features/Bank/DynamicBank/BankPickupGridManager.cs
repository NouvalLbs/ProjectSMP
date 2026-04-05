using System;
using System.Collections.Generic;

namespace ProjectSMP.Features.Bank.DynamicBank
{
    internal static class BankPickupGridManager
    {
        private const float GridSize = 100.0f;
        private const int GridCells = 100;

        private static readonly List<int>[,] Grid = new List<int>[GridCells, GridCells];

        public static void Initialize()
        {
            for (var x = 0; x < GridCells; x++)
                for (var y = 0; y < GridCells; y++)
                    Grid[x, y] = new List<int>();
        }

        public static void Add(int id, float x, float y)
        {
            GetCell(x, y, out var cx, out var cy);
            if (!Grid[cx, cy].Contains(id))
                Grid[cx, cy].Add(id);
        }

        public static void Remove(int id, float x, float y)
        {
            GetCell(x, y, out var cx, out var cy);
            Grid[cx, cy].Remove(id);
        }

        public static List<int> GetInCell(float x, float y)
        {
            GetCell(x, y, out var cx, out var cy);
            return new List<int>(Grid[cx, cy]);
        }

        private static void GetCell(float x, float y, out int cx, out int cy)
        {
            cx = Math.Clamp((int)Math.Floor(x / GridSize), 0, GridCells - 1);
            cy = Math.Clamp((int)Math.Floor(y / GridSize), 0, GridCells - 1);
        }
    }
}