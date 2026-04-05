using System;
using SampSharp.GameMode.SAMP;

namespace ProjectSMP.Plugins.EVF2
{
    public enum EVFParamType { Engine, Lights, Alarm, Doors, Bonnet, Boot, Objective }
    public enum EVFDamageType { Panels, Doors, Lights, Tires }
    public enum EVFVehicleBodyPart { Unknown, FrontLeftWheel, FrontRightWheel, BackLeftWheel, BackRightWheel, PetrolCap }
    public enum EVFBlinkSide { None = -1, Left = 0, Right = 1, Emergency = 2 }

    public class EVFVehicleData
    {
        public float VelX, VelY, VelZ;
        public float PosX, PosY, PosZ, PosAngle;
        public float Health = 1000f;
        public int Uptime;

        public int Paintjob = 3;
        public int Interior;
        public int Color1 = -1, Color2 = -1;
        public int Horn;
        public float SpawnX, SpawnY, SpawnZ, SpawnAngle;
        public int SpawnWorld, SpawnInterior;
        public float SpeedCap;
        public bool FuelEnabled;
        public int Fuel = 1000;
        public bool Sticky;
        public bool UnoccupiedDamage;
        public bool DamagePanels = true;
        public bool DamageDoors = true;
        public bool DamageLights = true;
        public bool DamageTires = true;
        public int Bomb;
        public Timer BombTimer;
        public bool Bulletproof;
        public bool Stored;
        public int TrailerId = -1;
        public EVFBlinkSide BlinkSide = EVFBlinkSide.None;
        public float BlinkAngle;
        
        public SampSharp.Streamer.World.DynamicObject[][] Neons = new SampSharp.Streamer.World.DynamicObject[3][];
        public SampSharp.Streamer.World.DynamicObject[] Blinks = new SampSharp.Streamer.World.DynamicObject[4];
        
        public SampSharp.Streamer.World.DynamicObject EditorObject;
        public SampSharp.Streamer.World.DynamicObject[] AttachedObjects = new SampSharp.Streamer.World.DynamicObject[30];

        public EVFVehicleData()
        {
            for (int i = 0; i < 3; i++) Neons[i] = new SampSharp.Streamer.World.DynamicObject[2];
        }
    }

    public class EVFPlayerData
    {
        public int EditorVehicleId = -1;
        public bool InModShop;
        public SampSharp.Streamer.World.DynamicObject EditorObject;
    }

    public class EVFTrailerEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public int VehicleId { get; set; }
        public int TrailerId { get; set; }
    }

    public class EVFVehicleFuelEventArgs : EventArgs
    {
        public int VehicleId { get; set; }
        public int NewFuel { get; set; }
        public int OldFuel { get; set; }
    }

    public class EVFBombEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public int VehicleId { get; set; }
    }

    public class EVFPlayerShotVehicleEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public int VehicleId { get; set; }
        public int WeaponId { get; set; }
        public float Damage { get; set; }
        public EVFVehicleBodyPart BodyPart { get; set; }
    }

    public class EVFSpeedCapEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public int VehicleId { get; set; }
        public float Speed { get; set; }
    }

    public class EVFVehicleModExEventArgs : EventArgs
    {
        public int PlayerId { get; set; }
        public int VehicleId { get; set; }
        public int ComponentId { get; set; }
        public int Price { get; set; }
        public bool Illegal { get; set; }
    }

    public class EVFVehiclePosChangeEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
        public int VehicleId { get; set; }
        public float NewX, NewY, NewZ, NewAngle;
        public float OldX, OldY, OldZ, OldAngle;
    }

    public class EVFVehicleVelocityChangeEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
        public int VehicleId { get; set; }
        public float NewX, NewY, NewZ;
        public float OldX, OldY, OldZ;
    }

    public class EVFVehicleHealthChangeEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
        public int VehicleId { get; set; }
        public float NewHealth { get; set; }
        public float OldHealth { get; set; }
    }
}