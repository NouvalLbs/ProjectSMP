using SampSharp.GameMode.Display;

namespace ProjectSMP.Entities.Players.Needs.Data
{
    internal sealed class PlayerNeedsData
    {
        public PlayerTextDraw[,] HudComponents { get; set; }
        public string[] ActiveHudNames { get; set; }
        public float[] ActiveHudCoords { get; set; }
        public int ActiveHudCount { get; set; }

        public PlayerNeedsData()
        {
            HudComponents = new PlayerTextDraw[5, 5];
            ActiveHudNames = new string[5];
            ActiveHudCoords = new float[5];
            ActiveHudCount = 0;
        }
    }
}