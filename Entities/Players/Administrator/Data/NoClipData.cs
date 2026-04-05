using SampSharp.GameMode.World;

namespace ProjectSMP.Entities.Players.Administrator.Data
{
    public enum NoClipMode
    {
        None = 0,
        Fly = 1
    }

    public enum MoveDirection
    {
        Forward = 1,
        Back = 2,
        Left = 3,
        Right = 4,
        ForwardLeft = 5,
        ForwardRight = 6,
        BackLeft = 7,
        BackRight = 8
    }

    public class NoClipData
    {
        public NoClipMode Mode { get; set; }
        public PlayerObject FlyObject { get; set; }
        public MoveDirection Direction { get; set; }
        public int LastLR { get; set; }
        public int LastUD { get; set; }
        public int LastMove { get; set; }
        public float AccelMultiplier { get; set; }
    }
}