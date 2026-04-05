#nullable enable
using System;
using System.Collections.Generic;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;

namespace ProjectSMP.Plugins.WeaponConfig
{
    public enum DamageType { Multiplier = 0, Static = 1, RangeMultiplier = 2, Range = 3 }
    public enum LagCompMode { Disabled = 0, Enabled = 1, Strict = 2 }

    public enum HitRejectReason
    {
        NoIssuer, InvalidWeapon, LastShotInvalid, MultiplePlayers,
        MultiplePlayersShotgun, DyingPlayer, SameTeam, Unstreamed,
        InvalidHitType, BeingResynced, NotSpawned, OutOfRange,
        TooFarFromShot, ShootRateTooFast, ShootRateTooFastMultiple,
        HitRateTooFast, HitRateTooFastMultiple, TooFarFromOrigin,
        InvalidDamage, SameVehicle, OwnVehicle, InvalidVehicle, Disconnected
    }

    public class RangeDamageStep
    {
        public float Range { get; set; }
        public float Damage { get; set; }
    }

    public class WeaponEntry
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public float Damage { get; set; }
        public DamageType Type { get; set; } = DamageType.Static;
        public float Range { get; set; }
        public int MaxShootRate { get; set; }
        public bool AffectsArmour { get; set; } = true;
        public bool TorsoOnly { get; set; } = true;
        public List<RangeDamageStep> RangeSteps { get; set; } = new();
    }

    public class WeaponConfig
    {
        public LagCompMode LagCompensation { get; set; } = LagCompMode.Enabled;
        public bool VehiclePassengerDamage { get; set; } = false;
        public bool VehicleUnoccupiedDamage { get; set; } = false;
        public bool CbugAllowed { get; set; } = true;
        public bool CbugDeathDelay { get; set; } = true;
        public bool CustomFallDamage { get; set; } = false;
        public float FallDamageMultiplier { get; set; } = 25.0f;
        public float FallDeathVelocity { get; set; } = -0.6f;
        public int RespawnTime { get; set; } = 3000;
        public int MaxShootRateSamples { get; set; } = 4;
        public int MaxHitRateSamples { get; set; } = 4;
        public bool GlobalArmourRules { get; set; } = false;
        public bool GlobalTorsoRules { get; set; } = false;
        public bool EnableDamageFeed { get; set; } = true;
        public int DamageFeedHideDelay { get; set; } = 3000;
        public bool EnableHealthBar { get; set; } = true;
        public int DamageTakenSound { get; set; } = 1190;
        public int DamageGivenSound { get; set; } = 17802;
        public bool CustomVendingMachines { get; set; } = true;
        public int DeathSkipTimeout { get; set; } = 500;
        public int MaxPreviousHits { get; set; } = 10;
        public float PlayerStreamDistance { get; set; } = 200f;
        public float MaxDistFromShot { get; set; } = 5f;
        public float MaxDistFromOrigin { get; set; } = 5f;
        public int ShotTimeoutMs { get; set; } = 1000;
    }

    public class WeaponConfigRoot
    {
        public WeaponConfig Config { get; set; } = new();
        public List<WeaponEntry> Weapons { get; set; } = new();
    }

    public sealed class ResyncSnapshot
    {
        public float Health;
        public float Armour;
        public int Skin;
        public int Team;
        public Vector3 Position;
        public float FacingAngle;
        public (Weapon Weapon, int Ammo)[] Weapons = new (Weapon, int)[13];
    }

    public sealed class RejectedHit
    {
        public long Time { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Weapon { get; set; }
        public HitRejectReason Reason { get; set; }
        public float Info1 { get; set; }
        public float Info2 { get; set; }
        public float Info3 { get; set; }
        public string TargetName { get; set; } = "";
    }

    public sealed class SpawnClassInfo
    {
        public int Skin { get; set; }
        public int Team { get; set; }
        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public Weapon Weapon1 { get; set; }
        public int Ammo1 { get; set; }
        public Weapon Weapon2 { get; set; }
        public int Ammo2 { get; set; }
        public Weapon Weapon3 { get; set; }
        public int Ammo3 { get; set; }
    }

    public sealed class PreviousHitInfo
    {
        public int Tick;
        public int Issuer;
        public int Weapon;
        public float Amount;
        public float Health;
        public float Armour;
        public int Bodypart;
    }

    public class RejectedHitFormatted
    {
        public string FormattedString { get; set; } = "";
        public long Time { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }
        public int Weapon { get; set; }
        public HitRejectReason Reason { get; set; }
        public string TargetName { get; set; } = "";
    }

    public class PlayerDamageArgs : EventArgs
    {
        public Player Player { get; set; } = null!;
        public Player? Issuer { get; set; }
        public float Amount { get; set; }
        public int Weapon { get; init; }
        public int Bodypart { get; init; }
        public bool Cancel { get; set; }
    }

    public class RejectedHitArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public int Weapon { get; init; }
        public HitRejectReason Reason { get; init; }
        public float Info1 { get; init; }
        public float Info2 { get; init; }
        public float Info3 { get; init; }
        public string TargetName { get; init; } = "";
    }

    public class PrepareDeathArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public string AnimLib { get; set; } = "PED";
        public string AnimName { get; set; } = "DEAD_chest";
        public bool AnimLock { get; set; } = true;
        public int RespawnTime { get; set; }
        public bool Cancel { get; set; }
    }

    public class DeathFinishedArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public bool Cancelable { get; init; }
    }

    public class InvalidWeaponDamageArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public Player? Damaged { get; init; }
        public float Amount { get; init; }
        public int Weapon { get; init; }
        public int Bodypart { get; init; }
        public int Error { get; init; }
        public bool Given { get; init; }
    }

    public class SpawnInfo
    {
        public int Skin { get; set; }
        public int Team { get; set; }
        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public Weapon Weapon1 { get; set; }
        public int Ammo1 { get; set; }
        public Weapon Weapon2 { get; set; }
        public int Ammo2 { get; set; }
        public Weapon Weapon3 { get; set; }
        public int Ammo3 { get; set; }
    }

    public class VendingMachineArgs : EventArgs
    {
        public Player Player { get; init; } = null!;
        public float HealthGiven { get; set; } = 5f;
        public bool Cancel { get; set; }
    }

    public static class WeaponConfigConstants
    {
        public const int BodyPartUnknown = 0;
        public const int BodyPartTorso = 3;
        public const int BodyPartGroin = 4;
        public const int BodyPartLeftArm = 5;
        public const int BodyPartRightArm = 6;
        public const int BodyPartLeftLeg = 7;
        public const int BodyPartRightLeg = 8;
        public const int BodyPartHead = 9;
    }
}