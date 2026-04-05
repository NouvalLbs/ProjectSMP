using SampSharp.GameMode.Definitions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Plugins.Anticheat.State;

public class PlayerAcState
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public float Health { get; set; }
    public float Armour { get; set; }
    public int SetHealth { get; set; } = -1;
    public int SetArmour { get; set; } = -1;

    public int Money { get; set; }
    public long MoneyGivenTick { get; set; }

    public float LastFacingAngle { get; set; } = -1f;
    public int CamMode { get; set; }
    public int Anim { get; set; }

    public int SetSpecialActionId { get; set; } = -1;

    public int[] Weapons { get; } = new int[13];
    public int[] Ammo { get; } = new int[13];
    public int[] PreShotAmmo { get; } = new int[13];
    public long[] SetWeaponTick { get; } = new long[13];
    public long[] GiveAmmoTick { get; } = new long[13];
    public long[] ShotAmmoTick { get; } = new long[13];

    public int SpawnWeapon1 { get; set; }
    public int SpawnWeapon2 { get; set; }
    public int SpawnWeapon3 { get; set; }
    public int SpawnAmmo1 { get; set; }
    public int SpawnAmmo2 { get; set; }
    public int SpawnAmmo3 { get; set; }
    public float SpawnPosX { get; set; }
    public float SpawnPosY { get; set; }
    public float SpawnPosZ { get; set; }
    public bool HasSpawnPos { get; set; }

    public int VehicleId { get; set; } = -1;
    public int LastVehicleModel { get; set; }

    public string IpAddress { get; set; } = string.Empty;

    public long PutInVehicleTick { get; set; }
    public float PutInVehiclePosX { get; set; }
    public float PutInVehiclePosY { get; set; }
    public float PutInVehiclePosZ { get; set; }
    public long SetHealthTick { get; set; }
    public long DamageTick { get; set; }
    public long SetArmourTick { get; set; }
    public long SetSpecialActionTick { get; set; }
    public long ResetWeaponsTick { get; set; }
    public long ReloadTick { get; set; }
    public long RemoveFromVehicleTick { get; set; }
    public long VehicleVelocityTick { get; set; }
    public long PlayerVelocityTick { get; set; }
    public long SetPosTick { get; set; }
    public long SpectateTick { get; set; }
    public long SpawnTick { get; set; }
    public long UpdateTick { get; set; }
    public long EnterVehicleTick { get; set; }

    public int SpawnSetFlag { get; set; }
    public bool IsOnline { get; set; }
    public bool IsDead { get; set; }
    public bool IsSpectating { get; set; }
    public bool IsInModShop { get; set; }
    public bool IsFrozen { get; set; }
    public bool StuntBonusEnabled { get; set; } = true;
    public bool IsParachuting { get; set; }
    public float DropJpX { get; set; }
    public float DropJpY { get; set; }
    public long DropJpTick { get; set; }
    public bool WasJetpacking { get; set; }
    public bool PendingDamageResult { get; set; }
    public bool PendingVehicleDamageResult { get; set; }
    public bool PendingClassResult { get; set; }

    // ── Anti-NOP: GivePlayerWeapon ───────────────────────────────────────
    public int[] NopSetWeapon { get; } = Enumerable.Repeat(-1, 13).ToArray();
    public long[] NopSetWeaponDeadline { get; } = new long[13];

    // ── Anti-NOP: SetPlayerAmmo ──────────────────────────────────────────
    public int[] NopSetAmmoWeapon { get; } = Enumerable.Repeat(-1, 13).ToArray();
    public int[] NopSetAmmoExpected { get; } = new int[13];
    public long[] NopSetAmmoDeadline { get; } = new long[13];

    // ── Anti-NOP: SetPlayerInterior ──────────────────────────────────────
    public int NopSetInteriorExpected { get; set; } = -1;
    public long NopSetInteriorDeadline { get; set; }

    // ── Anti-NOP: SetPlayerHealth ────────────────────────────────────────
    public float NopSetHealthExpected { get; set; } = -1f;
    public long NopSetHealthDeadline { get; set; }

    // ── Anti-NOP: SetPlayerArmour ────────────────────────────────────────
    public float NopSetArmourExpected { get; set; } = -1f;
    public long NopSetArmourDeadline { get; set; }

    // ── Anti-NOP: SetPlayerSpecialAction ────────────────────────────────
    public int NopSetSpecialActionExpected { get; set; } = -1;
    public long NopSetSpecialActionDeadline { get; set; }

    // ── Anti-NOP: PutPlayerInVehicle ─────────────────────────────────────
    public int NopPutInVehicleExpected { get; set; } = -1;
    public long NopPutInVehicleDeadline { get; set; }
    public int NopPutInVehicleSeat { get; set; } = -1;

    // ── Anti-NOP: TogglePlayerSpectating ────────────────────────────────
    public int NopToggleSpectatingExpected { get; set; } = -1;
    public long NopToggleSpectatingDeadline { get; set; }

    // ── Anti-NOP: SpawnPlayer ────────────────────────────────────────────
    public bool NopSpawnPlayerPending { get; set; }
    public long NopSpawnPlayerDeadline { get; set; }

    // ── Anti-NOP: SetPlayerPos ───────────────────────────────────────────
    public bool NopSetPosPending { get; set; }
    public float NopSetPosX { get; set; }
    public float NopSetPosY { get; set; }
    public float NopSetPosZ { get; set; }
    public long NopSetPosDeadline { get; set; }
    public bool TpToZ { get; set; }

    // ── Anti-NOP: RemovePlayerFromVehicle ────────────────────────────────
    public bool NopRemoveFromVehiclePending { get; set; }
    public long NopRemoveFromVehicleDeadline { get; set; }

    private readonly ConcurrentDictionary<string, byte> _disabledChecks = new();
    public void DisableCheck(string name) => _disabledChecks.TryAdd(name, 0);
    public void EnableCheck(string name) => _disabledChecks.TryRemove(name, out _);
    public bool IsCheckEnabled(string name) => !_disabledChecks.ContainsKey(name);

    public ConcurrentDictionary<string, int> WarningCounts { get; } = new();
    public int GetWarning(string check) => WarningCounts.GetValueOrDefault(check);
    public int AddWarning(string check) => WarningCounts.AddOrUpdate(check, 1, (_, v) => v + 1);
    public void ResetWarning(string check) => WarningCounts.TryRemove(check, out _);
    public void ResetAllWarnings() => WarningCounts.Clear();
}