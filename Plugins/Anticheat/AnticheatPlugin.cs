#nullable enable
using ProjectSMP.Plugins.Anticheat.Checks.AntiCrash;
using ProjectSMP.Plugins.Anticheat.Checks.AntiFlood;
using ProjectSMP.Plugins.Anticheat.Checks.AntiNop;
using ProjectSMP.Plugins.Anticheat.Checks.Combat;
using ProjectSMP.Plugins.Anticheat.Checks.Movement;
using ProjectSMP.Plugins.Anticheat.Checks.Player;
using ProjectSMP.Plugins.Anticheat.Checks.Server;
using ProjectSMP.Plugins.Anticheat.Checks.Spawn;
using ProjectSMP.Plugins.Anticheat.Checks.Vehicle;
using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.Anticheat.Data;
using ProjectSMP.Plugins.Anticheat.Managers;
using ProjectSMP.Plugins.Anticheat.Utilities;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Timers;

namespace ProjectSMP.Plugins.Anticheat;

public class AnticheatPlugin : IDisposable
{
    private readonly bool _weaponConfigMode;
    private readonly PlayerStateManager _players;
    private readonly VehicleStateManager _vehicles;
    private readonly PickupStateManager _pickups;
    private readonly WarningManager _warnings;
    private readonly FloodRateLimiter _flood;
    private readonly AcLogger _logger;
    private readonly AnticheatConfig _config;
    private FileSystemWatcher? _watcher;
    private System.Timers.Timer? _timer;

    private AirBreakCheck _airBreak = null!;
    private TeleportCheck _teleport = null!;
    private SpeedHackCheck _speedHack = null!;
    private FlyHackCheck _flyHack = null!;
    private HealthCheck _health = null!;
    private ArmourCheck _armour = null!;
    private MoneyCheck _money = null!;
    private WeaponCheck _weapon = null!;
    private AmmoCheck _ammo = null!;
    private GodModeCheck _godMode = null!;
    private SpecialActionCheck _specialAction = null!;
    private InvisibleCheck _invisible = null!;
    private FakeSpawnCheck _fakeSpawn = null!;
    private FakeKillCheck _fakeKill = null!;
    private RapidFireCheck _rapidFire = null!;
    private ProAimCheck _proAim = null!;
    private QuickTurnCheck _quickTurn = null!;
    private LagCompSpoofCheck _lagComp = null!;
    private CarShotCheck _carShot = null!;
    private FullAimingCheck _fullAiming = null!;
    private CjRunCheck _cjRun = null!;
    private CarJackCheck _carJack = null!;
    private VehicleTeleportCheck _vehicleTeleport = null!;
    private TuningHackCheck _tuningHack = null!;
    private ReconnectCheck _reconnect = null!;
    private PingCheck _ping = null!;
    private VersionCheck _version = null!;
    private SandboxProtection _sandbox = null!;
    private RconProtection _rcon = null!;
    private TuningCrasherCheck _tuningCrasher = null!;
    private InvalidSeatCrasherCheck _seatCrasher = null!;
    private DialogCrasherCheck _dialogCrasher = null!;
    private AttachedObjectCrasherCheck _attachCrasher = null!;
    private WeaponCrasherCheck _weaponCrasher = null!;
    private ConnectionFloodCheck _connFlood = null!;
    private CallbackFloodCheck _cbFlood = null!;
    private SeatFloodCheck _seatFlood = null!;
    private DosCheck _dos = null!;
    private ParkourModCheck _parkourMod = null!;
    private UnFreezeCheck _unFreeze = null!;
    private FakeNpcCheck _fakeNpc = null!;
    private AfkGhostCheck _afkGhost = null!;
    private CameraHackCheck _cameraHack = null!;
    private AnimationHackCheck _animationHack = null!;

    // ── Anti-NOP ─────────────────────────────────────────────────────────
    private NopGiveWeaponCheck _nopGiveWeapon = null!;
    private NopSetAmmoCheck _nopSetAmmo = null!;
    private NopSetInteriorCheck _nopSetInterior = null!;
    private NopSetHealthCheck _nopSetHealth = null!;
    private NopSetVehicleHealthCheck _nopSetVehicleHealth = null!;
    private NopSetArmourCheck _nopSetArmour = null!;
    private NopSetSpecialActionCheck _nopSetSpecialAction = null!;
    private NopPutPlayerInVehicleCheck _nopPutInVehicle = null!;
    private NopToggleSpectatingCheck _nopToggleSpectating = null!;
    private NopSpawnPlayerCheck _nopSpawnPlayer = null!;
    private NopSetPlayerPosCheck _nopSetPos = null!;
    private NopRemoveFromVehicleCheck _nopRemoveFromVehicle = null!;

    public PlayerStateManager Players => _players;
    public WarningManager Warnings => _warnings;
    public AnticheatConfig Config => _config;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public AnticheatPlugin(PlayerStateManager players, VehicleStateManager vehicles,
        PickupStateManager pickups, WarningManager warnings, FloodRateLimiter flood,
        AcLogger logger, AnticheatConfig config, bool weaponConfigMode = false)
    {
        _players = players;
        _vehicles = vehicles;
        _pickups = pickups;
        _warnings = warnings;
        _flood = flood;
        _logger = logger;
        _config = config;
        _weaponConfigMode = weaponConfigMode;
    }

    private void InitChecks()
    {
        _airBreak = new AirBreakCheck(_players, _warnings, _config);
        _teleport = new TeleportCheck(_players, _warnings, _config);
        _speedHack = new SpeedHackCheck(_players, _vehicles, _warnings, _config);
        _flyHack = new FlyHackCheck(_players, _warnings, _config);
        _health = new HealthCheck(_players, _warnings, _config, this);
        _armour = new ArmourCheck(_players, _warnings, _config);
        _money = new MoneyCheck(_players, _warnings, _config);
        _weapon = new WeaponCheck(_players, _warnings, _config);
        _ammo = new AmmoCheck(_players, _warnings, _config);
        _godMode = new GodModeCheck(_players, _warnings, _config);
        _specialAction = new SpecialActionCheck(_players, _warnings, _config);
        _invisible = new InvisibleCheck(_players, _warnings, _config);
        _fakeSpawn = new FakeSpawnCheck(_players, _warnings, _config);
        _fakeKill = new FakeKillCheck(_players, _warnings, _config);
        _rapidFire = new RapidFireCheck(_players, _warnings, _config);
        _proAim = new ProAimCheck(_players, _warnings, _config);
        _quickTurn = new QuickTurnCheck(_players, _warnings, _config);
        _lagComp = new LagCompSpoofCheck(_players, _warnings, _config);
        _carShot = new CarShotCheck(_players, _warnings, _config);
        _fullAiming = new FullAimingCheck(_players, _warnings, _config);
        _cjRun = new CjRunCheck(_players, _warnings, _config);
        _carJack = new CarJackCheck(_players, _warnings, _config);
        _vehicleTeleport = new VehicleTeleportCheck(_players, _vehicles, _pickups, _warnings, _config);
        _tuningHack = new TuningHackCheck(_players, _warnings, _config);
        _reconnect = new ReconnectCheck(_warnings, _config, _logger);
        _ping = new PingCheck(_warnings, _config, _logger);
        _dialogCrasher = new DialogCrasherCheck(_warnings, _config);
        _version = new VersionCheck(_config, _logger);
        _sandbox = new SandboxProtection(_config, _logger);
        _rcon = new RconProtection(_warnings, _config, _logger);
        _tuningCrasher = new TuningCrasherCheck(_warnings, _config);
        _seatCrasher = new InvalidSeatCrasherCheck(_warnings, _config);
        _attachCrasher = new AttachedObjectCrasherCheck(_warnings, _config);
        _weaponCrasher = new WeaponCrasherCheck(_players, _warnings, _config);
        _connFlood = new ConnectionFloodCheck(_config, _logger);
        _cbFlood = new CallbackFloodCheck(_flood, _warnings, _config);
        _seatFlood = new SeatFloodCheck(_warnings, _config);
        _dos = new DosCheck(_config, _logger);
        _parkourMod = new ParkourModCheck(_players, _warnings, _config);
        _unFreeze = new UnFreezeCheck(_players, _warnings, _config);
        _fakeNpc = new FakeNpcCheck(_config, _logger);
        _afkGhost = new AfkGhostCheck(_players, _warnings, _config);
        _cameraHack = new CameraHackCheck(_players, _warnings, _config);
        _animationHack = new AnimationHackCheck(_players, _warnings, _config);

        _nopGiveWeapon = new NopGiveWeaponCheck(_players, _warnings, _config);
        _nopSetAmmo = new NopSetAmmoCheck(_players, _warnings, _config);
        _nopSetInterior = new NopSetInteriorCheck(_players, _warnings, _config);
        _nopSetHealth = new NopSetHealthCheck(_players, _warnings, _config);
        _nopSetVehicleHealth = new NopSetVehicleHealthCheck(_players, _vehicles, _warnings, _config);
        _nopSetArmour = new NopSetArmourCheck(_players, _warnings, _config);
        _nopSetSpecialAction = new NopSetSpecialActionCheck(_players, _warnings, _config);
        _nopPutInVehicle = new NopPutPlayerInVehicleCheck(_players, _warnings, _config);
        _nopToggleSpectating = new NopToggleSpectatingCheck(_players, _warnings, _config);
        _nopSpawnPlayer = new NopSpawnPlayerCheck(_players, _warnings, _config);
        _nopSetPos = new NopSetPlayerPosCheck(_players, _warnings, _config);
        _nopRemoveFromVehicle = new NopRemoveFromVehicleCheck(_players, _warnings, _config);
    }

    private void InitHotReload(string configPath)
    {
        string dir = Path.GetDirectoryName(Path.GetFullPath(configPath)) ?? ".";
        string file = Path.GetFileName(configPath);
        _watcher = new FileSystemWatcher(dir, file)
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };
        _watcher.Changed += (_, _) =>
        {
            try
            {
                var fresh = LoadConfig(configPath);
                _config.Enabled = fresh.Enabled;
                _config.MaxPing = fresh.MaxPing;
                _config.MaxConnectsPerIp = fresh.MaxConnectsPerIp;
                _config.MinReconnectSeconds = fresh.MinReconnectSeconds;
                _config.SpeedHackVehResetDelay = fresh.SpeedHackVehResetDelay;
                foreach (var (k, v) in fresh.Checks) _config.Checks[k] = v;
                _logger.Log("AntiCheat.json reloaded.");
            }
            catch (Exception ex) { _logger.LogWarn($"Hot-reload failed: {ex.Message}"); }
        };
    }

    public static AnticheatPlugin Create(string configPath = "scriptfiles/AntiCheat.json", bool weaponConfigMode = false) {
        var cfg = LoadConfig(configPath);
        var players = new PlayerStateManager();
        var vehicles = new VehicleStateManager();
        var pickups = new PickupStateManager();
        var logger = new AcLogger(cfg);
        var flood = new FloodRateLimiter();
        var warnings = new WarningManager(players, cfg, logger);
        var plugin = new AnticheatPlugin(players, vehicles, pickups, warnings, flood, logger, cfg, weaponConfigMode);

        if (weaponConfigMode)
        {
            plugin.ConfigureForWeaponConfig();
        }

        plugin.InitHotReload(configPath);
        return plugin;
    }

    public static AnticheatConfig LoadConfig(string path = "scriptfiles/AntiCheat.json")
    {
        if (!File.Exists(path))
        {
            var def = new AnticheatConfig();
            File.WriteAllText(path, JsonSerializer.Serialize(def, _jsonOptions));
            return def;
        }
        return JsonSerializer.Deserialize<AnticheatConfig>(File.ReadAllText(path), _jsonOptions)
               ?? new AnticheatConfig();
    }

    // ── Game Script Hooks ────────────────────────────────────────────────

    public void OnGivePlayerMoney(int playerId, int amount)
        => _money.AllowMoneyGain(playerId, amount);

    public void OnResetPlayerMoney(int playerId)
        => _money.OnResetPlayerMoney(playerId);

    public void OnGivePlayerWeapon(int playerId, int weaponId, int ammo)
    {
        _weapon.OnWeaponGiven(playerId, weaponId, ammo);
        _ammo.OnAmmoGiven(playerId, weaponId, ammo);
        _nopGiveWeapon.OnWeaponGiven(playerId, weaponId, ammo);
    }

    public void OnSetPlayerAmmo(int playerId, int weaponId, int ammo)
        => _nopSetAmmo.OnSetPlayerAmmo(playerId, weaponId, ammo);

    public void OnSetPlayerInterior(int playerId, int interiorId) {
        var p = BasePlayer.Find(playerId);
        if (p is null) return;

        int oldInterior = p.Interior;
        _nopSetInterior.OnSetPlayerInterior(playerId, interiorId);
    }

    public void OnResetPlayerWeapons(int playerId)
    {
        _weapon.OnWeaponsReset(playerId);
        _nopGiveWeapon.OnWeaponsReset(playerId);
        _nopSetAmmo.OnWeaponsReset(playerId);
    }

    public void OnSetPlayerSpecialAction(int playerId, int action) {
        _specialAction.OnSpecialActionSet(playerId, action);
        _nopSetSpecialAction.OnSetPlayerSpecialAction(playerId, action);
    }

    public void OnSetPlayerHealth(int playerId, float health)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SetHealth = (int)health;
        st.SetHealthTick = Environment.TickCount64;
        _nopSetHealth.OnSetPlayerHealth(playerId, health);
    }

    public void OnSetVehicleHealth(int vehicleId, float health)
        => _nopSetVehicleHealth.OnSetVehicleHealth(vehicleId, health);

    public void OnRepairVehicle(int vehicleId)
        => _nopSetVehicleHealth.OnRepairVehicle(vehicleId);

    public void OnUpdateVehicleDamageStatus(int vehicleId, int panels, int doors, int lights, int tires)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.Panels = panels;
        vst.Doors = doors;
        vst.Lights = lights;
        vst.Tires = tires;
    }

    public void OnChangeVehiclePaintjob(int vehicleId, int paintjobId)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.Paintjob = paintjobId;
    }

    public void OnLinkVehicleToInterior(int vehicleId, int interiorId)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.Interior = interiorId % 256;
    }

    public void OnSetVehicleParamsEx(int vehicleId, bool doorsLocked)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.DoorsLocked = doorsLocked;
    }

    public void OnSetVehicleParamsForPlayer(int vehicleId, int playerId, bool doorsLocked)
    {
        var vst = _vehicles.GetOrCreate(vehicleId);
        vst.DoorsLockedPerPlayer[playerId] = doorsLocked;
    }

    public void OnSetVehicleToRespawn(int vehicleId)
    {
        var vst = _vehicles.Get(vehicleId);
        if (vst is not null)
        {
            vst.NopSetHealthExpected = -1f;
            vst.Panels = 0;
            vst.Doors = 0;
            vst.Lights = 0;
            vst.Tires = 0;
            vst.Paintjob = 3;
        }

        var veh = BaseVehicle.Find(vehicleId);
        if (veh is not null) {
            var pos = veh.Position;
            GetVehicleZAngle(vehicleId, out float angle);
            _vehicles.SetSpawnPosition(vehicleId, pos.X, pos.Y, pos.Z, angle);
        }

        long graceUntil = Environment.TickCount64 + 2650;
        foreach (var (_, st) in _players.All)
        {
            if (st.VehicleId == vehicleId)
            {
                st.SetPosTick = graceUntil;
                st.VehicleVelocityTick = graceUntil;
                st.VehicleId = -1;
            }
        }
    }

    public void OnEnableStuntBonusForAll(bool enable)
    {
        foreach (var (_, st) in _players.All)
            st.StuntBonusEnabled = enable;
    }

    public void OnEnableStuntBonusForPlayer(int playerId, bool enable)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.StuntBonusEnabled = enable;
    }

    public void OnPlayerSpectatePlayerOrVehicle(int playerId) {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SpectateTick = Environment.TickCount64 + 2650;
    }

    public void OnSetPlayerArmour(int playerId, float armour)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SetArmour = (int)armour;
        st.SetArmourTick = Environment.TickCount64;
        _nopSetArmour.OnSetPlayerArmour(playerId, armour);
    }

    public void OnSetPlayerPos(int playerId, float x, float y, float z) {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SetPosTick = Environment.TickCount64;
        _nopSetPos.OnSetPlayerPos(playerId, x, y, z);
    }

    public void OnSetPlayerPosFindZ(int playerId, float x, float y, float z)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SetPosTick = Environment.TickCount64;
        st.TpToZ = true;
        _nopSetPos.OnSetPlayerPos(playerId, x, y, z);
    }

    public void OnSetVehiclePos(int vehicleId, float x, float y, float z)
    {
        long now = Environment.TickCount64;
        foreach (var (playerId, st) in _players.All)
        {
            if (st.VehicleId != vehicleId) continue;
            st.SetPosTick = now;
            st.VehicleVelocityTick = now;
            _nopSetPos.OnSetPlayerPos(playerId, x, y, z);
        }
    }

    public void OnRemovePlayerFromVehicle(int playerId)
        => _nopRemoveFromVehicle.OnRemovePlayerFromVehicle(playerId);

    public void OnPutPlayerInVehicle(int playerId, int vehicleId, int seatId = 0)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.VehicleId = vehicleId;
        st.PutInVehicleTick = Environment.TickCount64;
        var vpos = BaseVehicle.Find(vehicleId)?.Position;
        if (vpos is not null) {
            st.PutInVehiclePosX = vpos.Value.X;
            st.PutInVehiclePosY = vpos.Value.Y;
            st.PutInVehiclePosZ = vpos.Value.Z;
        }
        _nopPutInVehicle.OnPutPlayerInVehicle(playerId, vehicleId, seatId);
    }

    public void OnTogglePlayerSpectating(int playerId, bool toggle) {
        var st = _players.Get(playerId);
        if (st is not null) {
            st.SpectateTick = Environment.TickCount64;
            st.IsSpectating = toggle;
        }
        _nopToggleSpectating.OnTogglePlayerSpectating(playerId, toggle);
    }

    public void OnPlayerVelocitySet(int playerId)
    {
        var st = _players.Get(playerId);
        if (st is not null) st.PlayerVelocityTick = Environment.TickCount64;
    }

    public void OnVehicleVelocitySet(int vehicleId)
    {
        long now = Environment.TickCount64;
        foreach (var (_, st) in _players.All)
            if (st.VehicleId == vehicleId) st.VehicleVelocityTick = now;
    }

    public void OnSetAttachedObject(int playerId, int slot, int modelId)
    {
        var p = BasePlayer.Find(playerId);
        if (p is not null) _attachCrasher.ValidateAttachedObject(p, slot, modelId);
    }

    public void OnRegisterPickup(int pickupId, float x, float y, float z, int type = 0, int weapon = 0, int amount = 0) {
        _pickups.Register(pickupId, x, y, z, type, weapon, amount);
    }

    public void OnDestroyPickup(int pickupId) {
        _pickups.Remove(pickupId);
    }

    public void OnSpawnPlayer(int playerId) {
        var st = _players.Get(playerId);
        if (st is not null) st.SpawnSetFlag = 1;
        _nopSpawnPlayer.OnSpawnPlayer(playerId);
    }

    public void OnTogglePlayerControllable(int playerId, bool toggle)
        => _unFreeze.OnPlayerFrozen(playerId, !toggle);

    public void OnSetPlayerSpawnInfo(int playerId, float x, float y, float z, int weapon1, int ammo1, int weapon2, int ammo2, int weapon3, int ammo3)
    {
        var st = _players.Get(playerId);
        if (st is null) return;
        st.SpawnWeapon1 = weapon1; st.SpawnAmmo1 = ammo1;
        st.SpawnWeapon2 = weapon2; st.SpawnAmmo2 = ammo2;
        st.SpawnWeapon3 = weapon3; st.SpawnAmmo3 = ammo3;
        st.SpawnPosX = x;
        st.SpawnPosY = y;
        st.SpawnPosZ = z;
        st.HasSpawnPos = true;
    }

    private void ConfigureForWeaponConfig()
    {
        _config.GetCheck("HealthHackOnfoot").Enabled = false;
        _config.GetCheck("HealthHackVehicle").Enabled = false;
        _config.GetCheck("ArmourHack").Enabled = false;
        _config.GetCheck("GodModeOnfoot").Enabled = false;
        _config.GetCheck("GodModeVehicle").Enabled = false;
        _config.GetCheck("NopSetHealth").Enabled = false;
        _config.GetCheck("NopSetArmour").Enabled = false;
        _config.GetCheck("FakeKill").Enabled = false;
        _config.GetCheck("FakeSpawn").Enabled = false;

        _logger.Log("[AC] WeaponConfig Integration Mode Enabled");
    }

    // ── RegisterEvents ───────────────────────────────────────────────────

    public void RegisterEvents(BaseMode gm)
    {
        InitChecks();

        if (_weaponConfigMode) {
            Integration.WeaponConfigBridge.Initialize(_players);
        }

        gm.PlayerConnected += OnPlayerConnected;
        gm.PlayerDisconnected += OnPlayerDisconnected;
        gm.PlayerUpdate += OnPlayerUpdate;
        gm.PlayerSpawned += OnPlayerSpawned;
        gm.PlayerDied += OnPlayerDied;
        gm.PlayerTakeDamage += OnPlayerTakeDamage;
        gm.PlayerWeaponShot += OnPlayerWeaponShot;
        gm.PlayerEnterVehicle += OnPlayerEnterVehicle;
        gm.PlayerExitVehicle += OnPlayerExitVehicle;
        gm.PlayerStateChanged += OnPlayerStateChanged;
        gm.PlayerText += OnPlayerText;
        gm.PlayerCommandText += OnPlayerCommandText;
        gm.PlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
        gm.PlayerLeaveCheckpoint += OnPlayerLeaveCheckpoint;
        gm.PlayerEnterRaceCheckpoint += OnPlayerEnterRaceCheckpoint;
        gm.PlayerLeaveRaceCheckpoint += OnPlayerLeaveRaceCheckpoint;
        gm.PlayerRequestClass += OnPlayerRequestClass;
        gm.PlayerRequestSpawn += OnPlayerRequestSpawn;
        gm.PlayerPickUpPickup += OnPlayerPickUpPickup;
        gm.PlayerSelectedMenuRow += OnPlayerSelectedMenuRow;
        gm.PlayerExitedMenu += OnPlayerExitedMenu;
        gm.PlayerClickMap += OnPlayerClickMap;
        gm.PlayerClickPlayer += OnPlayerClickPlayer;
        gm.PlayerClickTextDraw += OnPlayerClickTextDraw;
        gm.PlayerClickPlayerTextDraw += OnPlayerClickPlayerTextDraw;
        // gm.PlayerSelectObject += OnPlayerSelectObject;
        gm.PlayerKeyStateChanged += OnPlayerKeyStateChange;
        gm.VehicleMod += OnVehicleMod;
        gm.PlayerEnterExitModShop += OnPlayerEnterExitModShop;
        gm.VehiclePaintjobApplied += OnVehiclePaintjob;
        gm.VehicleResprayed += OnVehicleRespray;
        gm.VehicleDied += OnVehicleDied;
        gm.VehicleDamageStatusUpdated += OnVehicleDamageStatusUpdated;
        gm.VehicleSirenStateChange += OnVehicleSirenStateChange;
        gm.DialogResponse += OnDialogResponse;
        gm.RconLoginAttempt += OnRconLoginAttempt;

        _timer = new System.Timers.Timer(5000);
        _timer.Elapsed += (_, _) => {
            _ping.Tick();
            _afkGhost.Tick();
        };
        _timer.AutoReset = true;
        _timer.Start();
    }

    // ── Event Handlers ───────────────────────────────────────────────────

    private void OnPlayerConnected(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_connFlood.OnPlayerConnected(p)) return;
        _sandbox.OnPlayerConnected(p);
        if (!_fakeNpc.OnPlayerConnected(p)) return;
        _version.OnPlayerConnected(p);
        _reconnect.OnPlayerConnected(p);
        var st = _players.GetOrCreate(p.Id);
        st.IsOnline = true;
        st.StuntBonusEnabled = true;
        st.IpAddress = p.IP;
        _logger.Log($"Player {p.Id} connected from {p.IP}");
    }

    private void OnPlayerDisconnected(object? sender, DisconnectEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _connFlood.OnPlayerDisconnected(p.IP);
        _reconnect.OnPlayerDisconnected(p);
        _sandbox.OnPlayerDisconnected(p);
        _rcon.OnPlayerDisconnected(p.Id);
        _rapidFire.OnPlayerDisconnected(p.Id);
        _ping.OnPlayerDisconnected(p.Id);
        _seatFlood.OnPlayerDisconnected(p.Id);
        _dos.OnPlayerDisconnected(p.Id);
        _flood.ClearPlayer(p.Id);
        _nopSpawnPlayer.OnPlayerDisconnected(p.Id);
        _nopRemoveFromVehicle.OnPlayerDisconnected(p.Id);
        _players.Remove(p.Id);
    }

    private void OnPlayerUpdate(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_config.Enabled) return;
        if (!_dos.OnPlayerUpdate(p)) return;
        _airBreak.OnPlayerUpdate(p);
        _teleport.OnPlayerUpdate(p);
        if (p.Vehicle is not null && p.State == PlayerState.Driving) {
            var veh = p.Vehicle;
            var vst = _vehicles.Get(veh.Id);
            if (vst is not null) {
                var vel = veh.Velocity;
                var pos = veh.Position;

                if (VehicleData.IsTrailer((int)veh.Model)) {
                    vst.TrSpeed = (int)(VectorMath.Speed(vel.X, vel.Y, vel.Z) * 100.0f);
                    vst.TrPosDiff = VectorMath.Dist(pos.X, pos.Y, pos.Z, vst.TrPosX, vst.TrPosY, vst.TrPosZ);
                    vst.TrPosX = pos.X;
                    vst.TrPosY = pos.Y;
                    vst.TrPosZ = pos.Z;
                    vst.TrVelX = vel.X;
                    vst.TrVelY = vel.Y;
                    vst.TrVelZ = vel.Z;
                } else {
                    vst.TrSpeed = -1;
                    vst.TrSpeedDiff = 0;
                }
            }
        }
        _speedHack.OnPlayerUpdate(p);
        _flyHack.OnPlayerUpdate(p);
        _health.OnPlayerUpdate(p);
        _armour.OnPlayerUpdate(p);
        _money.OnPlayerUpdate(p);
        _weapon.OnPlayerUpdate(p);
        _ammo.OnPlayerUpdate(p);
        _godMode.OnPlayerUpdate(p);
        _quickTurn.OnPlayerUpdate(p);
        _fullAiming.OnPlayerUpdate(p);
        _cjRun.OnPlayerUpdate(p);
        _weaponCrasher.OnPlayerUpdate(p);
        _specialAction.OnPlayerUpdate(p);
        _invisible.OnPlayerUpdate(p);
        _cameraHack.OnPlayerUpdate(p);
        _animationHack.OnPlayerUpdate(p);
        _parkourMod.OnPlayerUpdate(p);
        _unFreeze.OnPlayerUpdate(p);

        var st = _players.Get(p.Id);
        if (st is not null) {
            st.CamMode = (int)p.CameraMode;
            st.Anim = p.AnimationIndex;

            if (958 <= st.Anim && st.Anim <= 979)
                st.IsParachuting = true;
            else if (st.IsParachuting)
                st.IsParachuting = false;
        }

        _nopGiveWeapon.OnPlayerUpdate(p);
        _nopSetAmmo.OnPlayerUpdate(p);
        _nopSetInterior.OnPlayerUpdate(p);
        _nopSetHealth.OnPlayerUpdate(p);
        _nopSetVehicleHealth.OnPlayerUpdate(p);
        _nopSetArmour.OnPlayerUpdate(p);
        _nopSetSpecialAction.OnPlayerUpdate(p);
        _nopPutInVehicle.OnPlayerUpdate(p);
        _nopToggleSpectating.OnPlayerUpdate(p);
        _nopSpawnPlayer.OnPlayerUpdate(p);
        _nopSetPos.OnPlayerUpdate(p);
        _nopRemoveFromVehicle.OnPlayerUpdate(p);
    }

    private void OnPlayerSpawned(object? sender, SpawnEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 19)) return;
        _fakeSpawn.OnPlayerSpawned(p);
        _health.OnPlayerSpawned(p);
        _armour.OnPlayerSpawned(p);
        _money.OnPlayerSpawned(p);
        _weapon.OnPlayerSpawned(p);
        _afkGhost.OnPlayerSpawned(p.Id);

        _nopGiveWeapon.OnPlayerSpawned(p.Id);
        _nopSetAmmo.OnPlayerSpawned(p.Id);
        _nopSetInterior.OnPlayerSpawned(p.Id);
        _nopSetHealth.OnPlayerSpawned(p.Id);
        _nopSetArmour.OnPlayerSpawned(p.Id);
        _nopSetSpecialAction.OnPlayerSpawned(p.Id);
        _nopPutInVehicle.OnPlayerSpawned(p.Id);
        _nopToggleSpectating.OnPlayerSpawned(p.Id);
        _nopSpawnPlayer.OnPlayerSpawned(p.Id);
        _nopSetPos.OnPlayerSpawned(p.Id);
        _nopRemoveFromVehicle.OnPlayerSpawned(p.Id);
    }

    private void OnPlayerDied(object? sender, DeathEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _godMode.OnPlayerDied(p);
        _fakeKill.OnPlayerDied(p, e);
        _afkGhost.OnPlayerDied(p.Id);

        _nopSetHealth.OnPlayerDied(p.Id);
        _nopSetArmour.OnPlayerDied(p.Id);
        _nopSetSpecialAction.OnPlayerDied(p.Id);
        _nopPutInVehicle.OnPlayerDied(p.Id);
        _nopToggleSpectating.OnPlayerDied(p.Id);
        _nopSetPos.OnPlayerDied(p.Id);
        _nopRemoveFromVehicle.OnPlayerDied(p.Id);
    }

    private void OnPlayerTakeDamage(object? sender, DamageEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _godMode.OnPlayerTakeDamage(p, e);
        _lagComp.OnPlayerTakeDamage(p, e);
    }

    private void OnPlayerWeaponShot(object? sender, WeaponShotEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _ammo.OnPlayerWeaponShot(p, (int)e.Weapon);
        _rapidFire.OnPlayerWeaponShot(p, e);
        _proAim.OnPlayerWeaponShot(p, e);
        _carShot.OnPlayerWeaponShot(p, e);
    }

    private void OnPlayerEnterVehicle(object? sender, EnterVehicleEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_seatFlood.OnPlayerEnterVehicle(p)) return;
        if (!_cbFlood.Check(p, 6)) return;
        _vehicleTeleport.OnPlayerEnterVehicle(p, e);
        if (!_carJack.OnPlayerEnterVehicle(p, e)) return;
        _seatCrasher.OnPlayerEnterVehicle(p, e);
        var st = _players.Get(p.Id);
        if (st is null) return;
        st.VehicleId = e.Vehicle.Id;
        st.EnterVehicleTick = Environment.TickCount64;
        _nopSetPos.OnPlayerEnterVehicle(p.Id);
    }

    private void OnPlayerExitVehicle(object? sender, PlayerVehicleEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 7)) return;
        var st = _players.Get(p.Id);
        if (st is null) return;
        _nopPutInVehicle.OnPlayerExitVehicle(p.Id);
        st.RemoveFromVehicleTick = Environment.TickCount64;
        _nopSetPos.OnPlayerExitVehicle(p.Id);
        _nopRemoveFromVehicle.OnPlayerExitVehicle(p.Id);
        st.VehicleId = -1;
    }

    private void OnPlayerStateChanged(object? sender, StateEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 11)) return;
        _vehicleTeleport.OnPlayerStateChanged(p, e);
        var st = _players.Get(p.Id);
        if (st is null) return;
        if (e.NewState == PlayerState.OnFoot && e.OldState == PlayerState.Driving)
        {
            st.RemoveFromVehicleTick = Environment.TickCount64;
            st.VehicleId = -1;
        }
        if (e.NewState == PlayerState.Driving)
        {
            st.EnterVehicleTick = Environment.TickCount64;
            if (p.Vehicle is not null) st.VehicleId = p.Vehicle.Id;
        }
    }

    private void OnPlayerText(object? sender, TextEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 16)) e.SendToPlayers = false;
    }

    private void OnPlayerCommandText(object? sender, CommandTextEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 5);
    }

    private void OnPlayerEnterCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 17);
    }

    private void OnPlayerLeaveCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 18);
    }

    private void OnPlayerEnterRaceCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 21);
    }

    private void OnPlayerLeaveRaceCheckpoint(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 22);
    }

    private void OnPlayerRequestSpawn(object? sender, RequestSpawnEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 19);
    }

    private void OnPlayerPickUpPickup(object? sender, PickUpPickupEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 8)) return;
        _vehicleTeleport.OnPlayerPickUpPickup(p, e);
        var pickup = _pickups.Get(e.Pickup.Id);
        if (pickup is null) return;

        switch (pickup.Type)
        {
            case 1:
                _money.AllowMoneyGain(p.Id, pickup.Amount);
                break;
            case 2:
                var hst = _players.Get(p.Id);
                if (hst is not null) hst.SetHealthTick = Environment.TickCount64;
                break;
            case 3:
                var ast = _players.Get(p.Id);
                if (ast is not null) ast.SetArmourTick = Environment.TickCount64;
                break;
            case 4:
                _weapon.OnWeaponGiven(p.Id, pickup.Weapon, WeaponData.PickupAmmo[pickup.Weapon]);
                _ammo.OnAmmoGiven(p.Id, pickup.Weapon, WeaponData.PickupAmmo[pickup.Weapon]);
                break;
        }
    }

    private void OnPlayerRequestClass(object? sender, RequestClassEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 9);
    }

    private void OnPlayerSelectedMenuRow(object? sender, MenuRowEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 10)) return;
    }

    private void OnPlayerExitedMenu(object? sender, EventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 20)) return;
    }

    private void OnPlayerClickMap(object? sender, PositionEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 2);
    }

    private void OnPlayerClickPlayer(object? sender, ClickPlayerEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 3);
    }

    private void OnPlayerClickTextDraw(object? sender, ClickTextDrawEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 4);
    }

    private void OnPlayerClickPlayerTextDraw(object? sender, ClickPlayerTextDrawEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 23);
    }

    /*
    private void OnPlayerSelectObject(object? sender, SelectObjectEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _cbFlood.Check(p, 26);
    }
    */

    private void OnPlayerKeyStateChange(object? sender, KeyStateChangedEventArgs e) {
        if (sender is not BasePlayer p) return;
    }

    private void OnVehicleMod(object? sender, VehicleModEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 12)) return;

        bool valid = _tuningCrasher.OnVehicleMod(v, p, e.ComponentId);
        if (valid)
        {
            _tuningHack.OnVehicleMod(v, p, e.ComponentId);
        }
    }

    private void OnPlayerEnterExitModShop(object? sender, EnterModShopEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 1)) return;
        var st = _players.Get(p.Id);
        if (st is null) return;
        st.IsInModShop = e.EnterExit == EnterExit.Entered;
    }

    private void OnVehiclePaintjob(object? sender, VehiclePaintjobEventArgs e) {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 13)) return;
    }

    private void OnVehicleRespray(object? sender, VehicleResprayedEventArgs e)
    {
        if (sender is not BaseVehicle) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 14);
    }

    private void OnVehicleDied(object? sender, PlayerEventArgs e)
    {
        if (sender is not BaseVehicle v) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 15);
        int model = (int)v.Model;
        long graceUntil = Environment.TickCount64 + 2650;
        foreach (var (_, st) in _players.All) {
            if (st.VehicleId == v.Id) {
                st.LastVehicleModel = model;
                st.SetPosTick = graceUntil;
                st.RemoveFromVehicleTick = graceUntil;
                st.VehicleId = -1;
            }
        }
        _vehicles.Remove(v.Id);
        _nopSetVehicleHealth.OnVehicleDestroyed(v.Id);
    }

    private void OnVehicleDamageStatusUpdated(object? sender, PlayerEventArgs e)
    {
        if (sender is not BaseVehicle) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 24);
    }

    private void OnVehicleSirenStateChange(object? sender, SirenStateEventArgs e)
    {
        if (sender is not BaseVehicle) return;
        if (e.Player is not BasePlayer p) return;
        _cbFlood.Check(p, 25);
    }

    private void OnDialogResponse(object? sender, DialogResponseEventArgs e) {
        if (sender is not BasePlayer p) return;
        if (!_cbFlood.Check(p, 0)) return;
        _dialogCrasher.OnDialogResponse(p, e);
    }

    private void OnRconLoginAttempt(object? sender, RconLoginAttemptEventArgs e)
    {
        if (sender is not BasePlayer p) return;
        _rcon.OnRconLoginAttempt(p, e.Password, e.SuccessfulLogin);
    }

    private void OnPunishment(int playerId, string checkName, PunishAction action)
    {
        var p = BasePlayer.Find(playerId);
        if (p is null) return;

        string message = $"Anticheat: {checkName}";

        switch (action)
        {
            case PunishAction.Kick:
                _logger.LogKick(playerId, checkName);
                p.SendClientMessage(Color.Red, message);
                p.Kick();
                break;

            case PunishAction.Ban:
                _logger.LogBan(playerId, checkName);
                p.Ban(message);
                break;
        }
    }

    public void UpdateCheckConfig(string checkName, Action<CheckConfig> configure) {
        var check = _config.GetCheck(checkName);
        configure(check);
        _logger.Log($"Check '{checkName}' configuration updated");
    }

    private void GetVehicleZAngle(int vehicleId, out float angle) {
        var veh = BaseVehicle.Find(vehicleId);
        if (veh is not null) {
            var rot = veh.Rotation;
            angle = rot.Z;
        } else {
            angle = 0.0f;
        }
    }

    public void Dispose() {
        if (_weaponConfigMode) {
            Integration.WeaponConfigBridge.Shutdown();
        }

        _timer?.Stop();
        _timer?.Dispose();
        _watcher?.Dispose();
        _logger.Dispose();
    }
}