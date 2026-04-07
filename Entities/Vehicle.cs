#nullable enable
using ProjectSMP.Entities.Vehicles.Handbrake;
using ProjectSMP.Extensions;
using ProjectSMP.Plugins.EVF2;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;

namespace ProjectSMP.Entities
{
    [PooledType]
    public partial class Vehicle : BaseVehicle
    {
        public int DatabaseId { get; set; } = -1;
        public VehicleType VehicleType { get; set; } = VehicleType.None;
        public string PlateText { get; set; } = "";
        public int[] ModComponents { get; set; } = new int[17];
        public TextLabel? VehicleLabel { get; set; }
        public DateTime LastUsed { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string CitizenId { get; set; } = "";
        public int Price { get; set; } = 0;
        public VehicleState State { get; set; } = VehicleState.Parked;

        public DateTime PlateTime { get; set; } = DateTime.MinValue;

        public int Ticket { get; set; } = 0;
        public DateTime TicketTime { get; set; } = DateTime.MinValue;

        public int Neon { get; set; } = 0;
        public int EngineUpgrade { get; set; } = 0;
        public int BodyUpgrade { get; set; } = 0;

        public bool IsInsured { get; set; } = false;
        public bool IsClaimed { get; set; } = false;
        public DateTime ClaimTime { get; set; } = DateTime.MinValue;

        public bool IsImpounded { get; set; } = false;
        public string ImpoundReason { get; set; } = "";

        public bool IsLockTire { get; set; } = false;

        public float VehicleHealth
        {
            get => EVFService.GetData(Id)?.Health ?? 1000f;
            set => EVFService.SetVehicleHealth(Id, value);
        }

        public bool IsLocked
        {
            get => EVFService.GetParam(Id, EVFParamType.Doors);
            set => EVFService.SwitchDoors(Id, value);
        }

        public bool IsEngineOn
        {
            get => EVFService.GetParam(Id, EVFParamType.Engine);
            set => EVFService.SwitchEngine(Id, value);
        }

        public int Fuel
        {
            get => EVFService.GetFuel(Id);
            set => EVFService.SetFuel(Id, value);
        }

        public int MaxFuel => EVFConstants.MaxVehicleFuel;

        public int CustomColor1
        {
            get => EVFService.GetData(Id)?.Color1 ?? -1;
            set { var d = EVFService.GetData(Id); if (d != null) d.Color1 = value; }
        }

        public int CustomColor2
        {
            get => EVFService.GetData(Id)?.Color2 ?? -1;
            set { var d = EVFService.GetData(Id); if (d != null) d.Color2 = value; }
        }

        public int CustomPaintjob
        {
            get => EVFService.GetPaintjob(Id);
            set => EVFService.ChangePaintjob(Id, value);
        }

        public Vector3 SpawnPosition
        {
            get { EVFService.GetSpawnInfo(Id, out float x, out float y, out float z, out _, out _, out _); return new Vector3(x, y, z); }
            set => EVFService.SetSpawnInfo(Id, value.X, value.Y, value.Z, SpawnRotation, SpawnWorld, SpawnInterior);
        }

        public float SpawnRotation
        {
            get { EVFService.GetSpawnInfo(Id, out _, out _, out _, out float a, out _, out _); return a; }
            set => EVFService.SetSpawnInfo(Id, SpawnPosition.X, SpawnPosition.Y, SpawnPosition.Z, value, SpawnWorld, SpawnInterior);
        }

        public int SpawnWorld
        {
            get { EVFService.GetSpawnInfo(Id, out _, out _, out _, out _, out int w, out _); return w; }
            set => EVFService.SetSpawnInfo(Id, SpawnPosition.X, SpawnPosition.Y, SpawnPosition.Z, SpawnRotation, value, SpawnInterior);
        }

        public int SpawnInterior
        {
            get { EVFService.GetSpawnInfo(Id, out _, out _, out _, out _, out _, out int i); return i; }
            set => EVFService.SetSpawnInfo(Id, SpawnPosition.X, SpawnPosition.Y, SpawnPosition.Z, SpawnRotation, SpawnWorld, value);
        }

        public int[] DamageStatus
        {
            get
            {
                var v = BaseVehicle.Find(Id);
                if (v == null) return new int[4];
                v.GetDamageStatus(out int p, out int d, out int l, out int t);
                return new[] { p, d, l, t };
            }
        }

        public override void OnDeath(PlayerEventArgs e)
        {
            base.OnDeath(e);
            WeaponConfigService.OnVehicleDeath(Id);
            OnVehicleDestroyed(e.Player as Player);
        }

        public override void OnStreamIn(PlayerEventArgs e)
        {
            base.OnStreamIn(e);
            OnVehicleStreamIn(e.Player as Player);
        }

        public override void OnStreamOut(PlayerEventArgs e)
        {
            base.OnStreamOut(e);
            OnVehicleStreamOut(e.Player as Player);
        }

        public override void OnPlayerEnter(EnterVehicleEventArgs e)
        {
            base.OnPlayerEnter(e);
            if (e.Player is Player player)
                OnPlayerEnterVehicle(player, e.IsPassenger);
        }

        public override void OnPlayerExit(PlayerVehicleEventArgs e)
        {
            base.OnPlayerExit(e);
            if (e.Player is Player player)
                OnPlayerExitVehicle(player);
        }

        public override void OnMod(VehicleModEventArgs e)
        {
            base.OnMod(e);
            var (valid, price) = EVFService.ValidateVehicleMod(e.Player?.Id ?? -1, Id, e.ComponentId);
            if (!valid) return;
            SaveModComponent(e.ComponentId);
            if (e.Player is Player player)
                OnVehicleModified(player, e.ComponentId);
        }

        public override void OnPaintjobApplied(VehiclePaintjobEventArgs e)
        {
            base.OnPaintjobApplied(e);
            if (e.Player is Player player)
                OnVehiclePaintjobChanged(player, e.PaintjobId);
        }

        public override void OnResprayed(VehicleResprayedEventArgs e)
        {
            base.OnResprayed(e);
        }

        public override void OnUnoccupiedUpdate(UnoccupiedVehicleEventArgs e)
        {
            base.OnUnoccupiedUpdate(e);
            if (e.Player is Player player)
                OnVehicleUnoccupiedUpdate(player);
        }

        protected virtual void OnPlayerEnterVehicle(Player? player, bool isPassenger) { }
        protected virtual void OnPlayerExitVehicle(Player? player)
        {
            if (player != null)
                HandbrakeService.OnPlayerExitVehicle(player, this);
        }
        protected virtual void OnVehicleDestroyed(Player? killer)
        {
            HandbrakeService.OnVehicleRemoved(Id);
        }
        protected virtual void OnVehicleStreamIn(Player? player) { }
        protected virtual void OnVehicleStreamOut(Player? player) { }
        protected virtual void OnVehicleModified(Player? player, int componentId) { }
        protected virtual void OnVehiclePaintjobChanged(Player? player, int paintjob) { }
        protected virtual void OnVehicleUnoccupiedUpdate(Player? player) { }

        public virtual void LockDoors(bool locked) => EVFService.SwitchDoors(Id, locked);

        public virtual void ToggleEngine(bool state) => EVFService.SwitchEngine(Id, state);

        public virtual void ToggleLights(bool state) => EVFService.SwitchLights(Id, state);

        public virtual bool GetEngineState() => EVFService.GetParam(Id, EVFParamType.Engine);

        public virtual bool GetLightsState() => EVFService.GetParam(Id, EVFParamType.Lights);

        public virtual bool GetDoorsLockedState() => EVFService.GetParam(Id, EVFParamType.Doors);

        public virtual void RefillFuel(int amount)
        {
            EVFService.SetFuel(Id, Math.Min(Fuel + amount, MaxFuel));
            UpdateVehicleLabel();
        }

        public virtual void ConsumeFuel(int amount)
        {
            EVFService.SetFuel(Id, Math.Max(Fuel - amount, 0));
            UpdateVehicleLabel();
        }

        public virtual bool HasFuel() => EVFService.GetFuel(Id) > 0;

        public virtual void SaveSpawnPoint()
        {
            EVFService.SetSpawnInfo(Id, Position.X, Position.Y, Position.Z, Angle, VirtualWorld, 0);
        }

        public virtual void RespawnAtSpawnPoint()
        {
            EVFService.GetSpawnInfo(Id, out float x, out float y, out float z, out float angle, out int world, out int interior);
            this.SetPositionSafe(new Vector3(x, y, z));
            Angle = angle;
            VirtualWorld = world;
            this.SetHealthSafe(VehicleHealth);
            ApplySpawnSettings();
            ApplyModComponents();
        }

        protected virtual void ApplySpawnSettings()
        {
            int pj = EVFService.GetPaintjob(Id);
            if (pj >= 0 && pj != EVFConstants.ResetPaintjobId)
                this.SetPaintjobSafe(pj);

            EVFService.GetColors(Id, out int c1, out int c2);
            if (c1 >= 0) ChangeColor(c1, c2);

            EVFService.SwitchDoors(Id, IsLocked);
            EVFService.SwitchEngine(Id, false);
        }

        public virtual void SaveModComponent(int componentId)
        {
            for (var i = 0; i < ModComponents.Length; i++)
            {
                if (ModComponents[i] == 0)
                {
                    ModComponents[i] = componentId;
                    break;
                }
            }
        }

        public virtual void ApplyModComponents()
        {
            foreach (var mod in ModComponents)
            {
                if (mod > 0)
                    AddComponent(mod);
            }
        }

        public virtual void CreateVehicleLabel()
        {
            DestroyVehicleLabel();
            var labelText = GetVehicleLabelText();
            if (!string.IsNullOrEmpty(labelText))
                VehicleLabel = new TextLabel(labelText, new Color(255, 255, 255), Position, 10f, 0, false);
        }

        public virtual void UpdateVehicleLabel()
        {
            if (VehicleLabel != null)
                VehicleLabel.Text = GetVehicleLabelText();
            else
                CreateVehicleLabel();
        }

        public virtual void DestroyVehicleLabel()
        {
            if (VehicleLabel != null)
            {
                VehicleLabel.Dispose();
                VehicleLabel = null;
            }
        }

        protected virtual string GetVehicleLabelText() => string.Empty;

        public virtual void ResetVehicle()
        {
            this.RepairSafe();
            EVFService.SetFuel(Id, MaxFuel);
            EVFService.SetVehicleHealth(Id, 1000f);
            EVFService.SwitchEngine(Id, false);
            EVFService.SwitchDoors(Id, false);
            UpdateVehicleLabel();
        }

        public virtual void Destroy()
        {
            DestroyVehicleLabel();
            WeaponConfigService.OnVehicleDestroy(Id);
            Dispose();
        }

        public static Vehicle CreateVehicle(VehicleModelType modelid, Vector3 position, float rotation, int color1, int color2, int respawnDelay = -1, bool addSiren = false)
        {
            var baseVehicle = Create(modelid, position, rotation, color1, color2, respawnDelay, addSiren);
            if (baseVehicle is not Vehicle vehicle)
            {
                baseVehicle.Dispose();
                throw new InvalidOperationException("Failed to create Vehicle instance");
            }

            WeaponConfigService.OnVehicleSpawn(vehicle.Id);
            EVFService.RegisterVehicle(vehicle.Id, (int)modelid, position, rotation, color1, color2, 0, 0, false);
            EVFService.SetFuelEnabled(vehicle.Id, true);
            vehicle.SaveSpawnPoint();
            vehicle.ApplySpawnSettings();
            vehicle.CreateVehicleLabel();
            return vehicle;
        }

        public static T CreateVehicle<T>(VehicleModelType modelid, Vector3 position, float rotation, int color1, int color2, int respawnDelay = -1, bool addSiren = false) where T : Vehicle, new()
        {
            var baseVehicle = Create(modelid, position, rotation, color1, color2, respawnDelay, addSiren);
            if (baseVehicle is not T vehicle)
            {
                baseVehicle.Dispose();
                throw new InvalidOperationException($"Failed to create {typeof(T).Name} instance");
            }

            WeaponConfigService.OnVehicleSpawn(vehicle.Id);
            EVFService.RegisterVehicle(vehicle.Id, (int)modelid, position, rotation, color1, color2, 0, 0, false);
            EVFService.SetFuelEnabled(vehicle.Id, true);
            vehicle.SaveSpawnPoint();
            vehicle.ApplySpawnSettings();
            vehicle.CreateVehicleLabel();
            return vehicle;
        }
    }

    public enum VehicleState
    {
        Parked,
        Spawned,
        Impounded,
        Stored
    }

    public enum VehicleType
    {
        None,
        Private,
        Faction,
        Business,
        Workshop,
        Job,
        Rental,
        Admin,
        Dealership
    }
}