using System.Collections.Concurrent;
namespace ProjectSMP.Plugins.Anticheat.State;
public class VehicleAcState {
    public int Paintjob { get; set; } = 3;
    public int Interior { get; set; }
    public int Panels { get; set; }
    public int Doors { get; set; }
    public int Lights { get; set; }
    public int Tires { get; set; }
    public bool DoorsLocked { get; set; }
    public ConcurrentDictionary<int, bool> DoorsLockedPerPlayer { get; } = new();
    public float NopSetHealthExpected { get; set; } = -1f;
    public long NopSetHealthDeadline { get; set; }
    public float TrVelX { get; set; }
    public float TrVelY { get; set; }
    public float TrVelZ { get; set; }
    public float TrPosX { get; set; }
    public float TrPosY { get; set; }
    public float TrPosZ { get; set; }
    public float TrPosDiff { get; set; }
    public int TrSpeed { get; set; } = -1;
    public int TrSpeedDiff { get; set; }
    public float SpawnPosX { get; set; }
    public float SpawnPosY { get; set; }
    public float SpawnPosZ { get; set; }
    public float SpawnZAngle { get; set; }
}