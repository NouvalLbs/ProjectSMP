using System;
using System.Collections.Generic;
using System.Linq;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;

namespace ProjectSMP.Plugins.EVF2
{
    public static class EVFService
    {
        private static readonly Dictionary<int, EVFVehicleData> _v = new();
        private static readonly Dictionary<int, EVFPlayerData> _p = new();
        private static readonly Random _rng = new();
        private static Timer _updateTimer;
        public static bool VehicleBlinking { get; set; } = false;

        public static event EventHandler<int> VehicleCreated;
        public static event EventHandler<int> VehicleDestroyed;
        public static event EventHandler<EVFTrailerEventArgs> TrailerHooked;
        public static event EventHandler<EVFTrailerEventArgs> TrailerUnhooked;
        public static event EventHandler<EVFVehicleFuelEventArgs> VehicleFuelChanged;
        public static event EventHandler<EVFBombEventArgs> BombActivated;
        public static event EventHandler<EVFBombEventArgs> BombExploded;
        public static event EventHandler<EVFBombEventArgs> BombDeactivated;
        public static event EventHandler<EVFPlayerShotVehicleEventArgs> PlayerShotVehicle;
        public static event EventHandler<EVFSpeedCapEventArgs> PlayerReachedSpeedCap;
        public static event EventHandler<EVFVehiclePosChangeEventArgs> VehiclePosChanged;
        public static event EventHandler<EVFVehicleVelocityChangeEventArgs> VehicleVelocityChanged;
        public static event EventHandler<EVFVehicleHealthChangeEventArgs> VehicleHealthChanged;
        public static event EventHandler<EVFVehicleModExEventArgs> VehicleModEx;

        // ── Init/Dispose ────────────────────────────────────────────────────
        public static void Initialize()
        {
            _updateTimer = new Timer(EVFConstants.VehicleUpdateInterval, true);
            _updateTimer.Tick += (s, e) => UpdateVehicles();
        }

        public static void Dispose()
        {
            _updateTimer?.Dispose();
            _v.Clear();
            _p.Clear();
        }

        // ── Vehicle Registration ────────────────────────────────────────────
        public static BaseVehicle CreateVehicle(int modelId, Vector3 pos, float angle,
            int color1 = -1, int color2 = -1, int respawnDelay = -1, bool addSiren = false,
            int worldId = 0, int interiorId = 0, bool unoccupiedDamage = false)
        {
            if (modelId < 400 || modelId > 611) return null;

            if (GetRandomColors(modelId, out var rc1, out var rc2))
            {
                if (color1 == -1) color1 = rc1;
                if (color2 == -1) color2 = rc2;
            }

            var vehicle = BaseVehicle.Create((VehicleModelType)modelId, pos, angle, color1, color2, respawnDelay, addSiren);
            if (vehicle == null) return null;

            RegisterVehicle(vehicle.Id, modelId, pos, angle, color1, color2, worldId, interiorId, unoccupiedDamage);

            vehicle.VirtualWorld = worldId;
            vehicle.LinkToInterior(interiorId);

            VehicleCreated?.Invoke(null, vehicle.Id);
            return vehicle;
        }

        public static void RegisterVehicle(int id, int modelId, Vector3 pos, float angle,
            int color1, int color2, int worldId, int interiorId, bool unoccupiedDamage)
        {
            var d = new EVFVehicleData
            {
                PosX = pos.X,
                PosY = pos.Y,
                PosZ = pos.Z,
                PosAngle = angle,
                SpawnX = pos.X,
                SpawnY = pos.Y,
                SpawnZ = pos.Z,
                SpawnAngle = angle,
                SpawnWorld = worldId,
                SpawnInterior = interiorId,
                Interior = interiorId,
                Color1 = color1,
                Color2 = color2,
                Fuel = EVFConstants.DefaultVehicleFuel,
                UnoccupiedDamage = unoccupiedDamage,
                Stored = true
            };
            _v[id] = d;
        }

        public static void DestroyVehicle(int id)
        {
            if (_v.TryGetValue(id, out var d))
            {
                d.BombTimer?.Dispose();
                _v.Remove(id);
            }
            VehicleDestroyed?.Invoke(null, id);
            BaseVehicle.Find(id)?.Dispose();
        }

        public static void RemoveVehicle(int id)
        {
            if (_v.TryGetValue(id, out var d)) d.Stored = false;
        }

        // ── Neons ─────────────────────────────────────────────────────────
        public static bool VehicleSupportsNeonLights(int modelId)
        {
            int i = modelId - 400;
            if (i < 0 || i >= EVFOffsets.NeonOffsetData.Length) return false;
            var o = EVFOffsets.NeonOffsetData[i];
            return !(o[0] == 0f && o[1] == 0f && o[2] == 0f);
        }

        public static void SetVehicleNeonLights(int id, bool enable = true, int colorModel = 18647, int slotId = 0)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            if (!VehicleSupportsNeonLights((int)v.Model)) return;
            var d = GetData(id); if (d == null) return;
            if (slotId < 0 || slotId > 2) return;

            if (enable)
            {
                if (d.Neons[slotId][0] != null) { d.Neons[slotId][0].Dispose(); d.Neons[slotId][0] = null; }
                if (d.Neons[slotId][1] != null) { d.Neons[slotId][1].Dispose(); d.Neons[slotId][1] = null; }

                if (colorModel != 0)
                {
                    int i = (int)v.Model - 400;
                    var o = EVFOffsets.NeonOffsetData[i];

                    d.Neons[slotId][0] = new SampSharp.Streamer.World.DynamicObject(colorModel, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    d.Neons[slotId][1] = new SampSharp.Streamer.World.DynamicObject(colorModel, new Vector3(0, 0, 0), new Vector3(0, 0, 0));

                    d.Neons[slotId][0].AttachTo(v, new Vector3(o[0], o[1], o[2]), new Vector3(0, 0, 0));
                    d.Neons[slotId][1].AttachTo(v, new Vector3(-o[0], o[1], o[2]), new Vector3(0, 0, 0));
                }
            }
            else
            {
                if (d.Neons[slotId][0] != null) { d.Neons[slotId][0].Dispose(); d.Neons[slotId][0] = null; }
                if (d.Neons[slotId][1] != null) { d.Neons[slotId][1].Dispose(); d.Neons[slotId][1] = null; }
            }
        }

        public static bool GetVehicleNeonLightsState(int id, int slotId = 0)
        {
            var d = GetData(id); if (d == null) return false;
            if (slotId < 0 || slotId > 2) return false;
            return d.Neons[slotId][0] != null && d.Neons[slotId][1] != null;
        }

        // ── Blinking ──────────────────────────────────────────────────────
        public static bool IsCarBlinking(int id) => GetData(id)?.BlinkSide != EVFBlinkSide.None;

        public static int DisableCarBlinking(int id)
        {
            var d = GetData(id); if (d == null || !IsCarBlinking(id)) return 0;
            for (int i = 0; i < 4; i++)
            {
                if (d.Blinks[i] != null)
                {
                    d.Blinks[i].Dispose();
                    d.Blinks[i] = null;
                }
            }
            d.BlinkSide = EVFBlinkSide.None;
            return 1;
        }

        public static void SetCarBlinking(int id, EVFBlinkSide side, bool skip = false)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            var d = GetData(id); if (d == null) return;

            if (IsCarBlinking(id) && !skip)
            {
                DisableCarBlinking(id);
                return;
            }

            if (!skip) d.BlinkSide = side;
            d.BlinkAngle = v.Angle;

            int trailerId = v.Trailer?.Id ?? -1;
            int modelId = (int)v.Model;
            int offsetIdx = modelId - 400;
            if (offsetIdx < 0 || offsetIdx >= EVFOffsets.BlinkOffsetData.Length) return;

            var o = EVFOffsets.BlinkOffsetData[offsetIdx];
            var b = d.Blinks;

            if (side == EVFBlinkSide.Left || side == EVFBlinkSide.Emergency)
            {
                if (o[0] != 0f)
                {
                    b[0] = new SampSharp.Streamer.World.DynamicObject(19294, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    b[2] = new SampSharp.Streamer.World.DynamicObject(19294, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    b[0].AttachTo(v, new Vector3(o[0], o[1], o[2]), new Vector3(0, 0, 0));
                    b[2].AttachTo(v, new Vector3(-o[0], o[1], o[2]), new Vector3(0, 0, 0));
                }
                if (o[3] != 0f)
                {
                    b[1] = new SampSharp.Streamer.World.DynamicObject(19294, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    b[3] = new SampSharp.Streamer.World.DynamicObject(19294, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    if (trailerId >= 0 && BaseVehicle.Find(trailerId) != null)
                    {
                        b[1].AttachTo(BaseVehicle.Find(trailerId), new Vector3(o[3], o[4], o[5]), new Vector3(0, 0, 0));
                        b[3].AttachTo(BaseVehicle.Find(trailerId), new Vector3(-o[3], o[4], o[5]), new Vector3(0, 0, 0));
                    }
                    else
                    {
                        b[1].AttachTo(v, new Vector3(o[3], o[4], o[5]), new Vector3(0, 0, 0));
                        b[3].AttachTo(v, new Vector3(-o[3], o[4], o[5]), new Vector3(0, 0, 0));
                    }
                }
            }
            if (side == EVFBlinkSide.Right)
            {
                if (o[0] != 0f)
                {
                    b[0] = new SampSharp.Streamer.World.DynamicObject(19294, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    b[0].AttachTo(v, new Vector3(-o[0], o[1], o[2]), new Vector3(0, 0, 0));
                }
                if (o[3] != 0f)
                {
                    b[1] = new SampSharp.Streamer.World.DynamicObject(19294, new Vector3(0, 0, 0), new Vector3(0, 0, 0));
                    if (trailerId >= 0 && BaseVehicle.Find(trailerId) != null)
                        b[1].AttachTo(BaseVehicle.Find(trailerId), new Vector3(-o[3], o[4], o[5]), new Vector3(0, 0, 0));
                    else
                        b[1].AttachTo(v, new Vector3(-o[3], o[4], o[5]), new Vector3(0, 0, 0));
                }
            }
        }

        // ── Data Access ────────────────────────────────────────────────────
        public static EVFVehicleData GetData(int id) => _v.TryGetValue(id, out var d) ? d : null;
        public static EVFPlayerData GetPlayerData(int id) { if (!_p.ContainsKey(id)) _p[id] = new EVFPlayerData(); return _p[id]; }
        public static int GetVehicleInterior(int id) => GetData(id)?.Interior ?? 0;

        public static void SetVehicleInterior(int id, int interiorId)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.LinkToInterior(interiorId);
            var d = GetData(id); if (d != null) d.Interior = interiorId;
        }

        // ── Fuel ───────────────────────────────────────────────────────────
        public static int GetFuel(int id) => GetData(id)?.Fuel ?? 0;
        public static void SetFuel(int id, int fuel) { var d = GetData(id); if (d != null) d.Fuel = Math.Clamp(fuel, 0, EVFConstants.MaxVehicleFuel); }
        public static bool IsFuelEnabled(int id) => GetData(id)?.FuelEnabled ?? false;
        public static void SetFuelEnabled(int id, bool on) { var d = GetData(id); if (d != null) d.FuelEnabled = on; }

        // ── Speed Cap ──────────────────────────────────────────────────────
        public static float GetSpeedCap(int id) => GetData(id)?.SpeedCap ?? 0f;
        public static void SetSpeedCap(int id, float cap) { var d = GetData(id); if (d != null) d.SpeedCap = cap; }
        public static void DisableSpeedCap(int id) => SetSpeedCap(id, 0f);

        // ── Sticky ────────────────────────────────────────────────────────
        public static bool IsSticky(int id) => GetData(id)?.Sticky ?? false;
        public static void SetSticky(int id, bool on) { var d = GetData(id); if (d != null) d.Sticky = on; }

        // ── Bulletproof ───────────────────────────────────────────────────
        public static bool IsBulletproof(int id) => GetData(id)?.Bulletproof ?? false;
        public static void SetBulletproof(int id, bool on) { var d = GetData(id); if (d != null) d.Bulletproof = on; }

        // ── Bomb ──────────────────────────────────────────────────────────
        public static int GetBomb(int id) => GetData(id)?.Bomb ?? 0;
        public static bool IsBombed(int id) => GetBomb(id) > 0;
        public static bool IsBombActivated(int id) => IsBombed(id) && GetData(id)?.BombTimer != null;

        public static void SetBomb(int id, int time = 1000)
        {
            var d = GetData(id); if (d == null) return;
            if (IsBombed(id))
            {
                SetParam(id, EVFParamType.Alarm, false);
                d.BombTimer?.Dispose();
                d.BombTimer = null;
            }
            d.Bomb = time;
        }

        public static void RemoveBomb(int id) => SetBomb(id, 0);

        // ── Horn ──────────────────────────────────────────────────────────
        public static int GetHorn(int id) => GetData(id)?.Horn ?? 0;
        public static void SetHorn(int id, int horn) { var d = GetData(id); if (d != null) d.Horn = horn; }
        public static void RestoreHorn(int id) => SetHorn(id, 0);

        // ── Unoccupied Damage ─────────────────────────────────────────────
        public static bool IsUnoccupiedDamageEnabled(int id) => GetData(id)?.UnoccupiedDamage ?? false;
        public static void SetUnoccupiedDamage(int id, bool on) { var d = GetData(id); if (d != null) d.UnoccupiedDamage = on; }

        // ── Colors / Paintjob ─────────────────────────────────────────────
        public static void ChangeColor(int id, int c1, int c2)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            var d = GetData(id);
            if (GetRandomColors((int)v.Model, out var rc1, out var rc2))
            {
                if (c1 == -1) c1 = rc1;
                if (c2 == -1) c2 = rc2;
            }
            v.ChangeColor(c1, c2);
            if (d != null) { d.Color1 = c1; d.Color2 = c2; }
        }

        public static void ChangePaintjob(int id, int paintjobId)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            var d = GetData(id);
            v.ChangePaintjob(paintjobId);
            if (d != null)
            {
                d.Paintjob = paintjobId;
                if (paintjobId == EVFConstants.ResetPaintjobId)
                    ChangeColor(id, d.Color1, d.Color2);
            }
        }

        public static void RemovePaintjob(int id) => ChangePaintjob(id, EVFConstants.ResetPaintjobId);
        public static int GetPaintjob(int id) => GetData(id)?.Paintjob ?? EVFConstants.ResetPaintjobId;
        public static int GetStoredInterior(int id) => GetData(id)?.Interior ?? 0;
        public static bool GetColors(int id, out int c1, out int c2)
        {
            var d = GetData(id);
            c1 = d?.Color1 ?? 0;
            c2 = d?.Color2 ?? 0;
            return d != null;
        }

        // ── Spawn Info ────────────────────────────────────────────────────
        public static void GetSpawnInfo(int id, out float x, out float y, out float z,
            out float angle, out int worldId, out int interiorId)
        {
            var d = GetData(id);
            x = d?.SpawnX ?? 0; y = d?.SpawnY ?? 0; z = d?.SpawnZ ?? 0;
            angle = d?.SpawnAngle ?? 0; worldId = d?.SpawnWorld ?? 0; interiorId = d?.SpawnInterior ?? 0;
        }

        public static void SetSpawnInfo(int id, float x, float y, float z, float angle, int worldId, int interiorId)
        {
            var d = GetData(id); if (d == null) return;
            d.SpawnX = x; d.SpawnY = y; d.SpawnZ = z; d.SpawnAngle = angle;
            d.SpawnWorld = worldId; d.SpawnInterior = interiorId;
        }

        // ── Vehicle Params ────────────────────────────────────────────────
        public static bool GetParam(int id, EVFParamType type)
        {
            var v = BaseVehicle.Find(id); if (v == null) return false;
            v.GetParameters(out bool eng, out bool lights, out bool alarm, out bool doors,
                out bool bonnet, out bool boot, out bool obj);
            return type switch
            {
                EVFParamType.Engine => eng,
                EVFParamType.Lights => lights,
                EVFParamType.Alarm => alarm,
                EVFParamType.Doors => doors,
                EVFParamType.Bonnet => bonnet,
                EVFParamType.Boot => boot,
                EVFParamType.Objective => obj,
                _ => false
            };
        }

        public static void SetParam(int id, EVFParamType type, bool value, int delayMs = 0)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.GetParameters(out bool eng, out bool lights, out bool alarm, out bool doors,
                out bool bonnet, out bool boot, out bool obj);
            switch (type)
            {
                case EVFParamType.Engine: eng = value; break;
                case EVFParamType.Lights: lights = value; break;
                case EVFParamType.Alarm: alarm = value; break;
                case EVFParamType.Doors: doors = value; break;
                case EVFParamType.Bonnet: bonnet = value; break;
                case EVFParamType.Boot: boot = value; break;
                case EVFParamType.Objective: obj = value; break;
            }
            if (delayMs > 0)
            {
                var t = new Timer(delayMs, false);
                t.Tick += (s, e) => { BaseVehicle.Find(id)?.SetParameters(eng, lights, alarm, doors, bonnet, boot, obj); t.Dispose(); };
            }
            else
                v.SetParameters(eng, lights, alarm, doors, bonnet, boot, obj);
        }

        public static void SwitchEngine(int id, bool on) => SetParam(id, EVFParamType.Engine, on);
        public static void SwitchLights(int id, bool on) => SetParam(id, EVFParamType.Lights, on);
        public static void SwitchAlarm(int id, bool on) => SetParam(id, EVFParamType.Alarm, on);
        public static void SwitchDoors(int id, bool locked) => SetParam(id, EVFParamType.Doors, locked);
        public static void SwitchBonnet(int id, bool open) => SetParam(id, EVFParamType.Bonnet, open);
        public static void SwitchBoot(int id, bool open) => SetParam(id, EVFParamType.Boot, open);
        public static void SwitchObjective(int id, bool on) => SetParam(id, EVFParamType.Objective, on);
        public static bool IsValidVehicleModelId(int modelId) => modelId is >= 400 and <= 611;

        // ── Damage Status ─────────────────────────────────────────────────
        public static bool IsDamageEnabled(int id, EVFDamageType type)
        {
            var d = GetData(id); if (d == null) return false;
            return type switch { EVFDamageType.Panels => d.DamagePanels, EVFDamageType.Doors => d.DamageDoors, EVFDamageType.Lights => d.DamageLights, EVFDamageType.Tires => d.DamageTires, _ => false };
        }

        public static void SetDamageEnabled(int id, EVFDamageType type, bool on)
        {
            var d = GetData(id); if (d == null) return;
            var v = BaseVehicle.Find(id);
            if (!on) { SetParam(id, EVFParamType.Engine, false); }
            switch (type) { case EVFDamageType.Panels: d.DamagePanels = on; break; case EVFDamageType.Doors: d.DamageDoors = on; break; case EVFDamageType.Lights: d.DamageLights = on; break; case EVFDamageType.Tires: d.DamageTires = on; break; }
        }

        public static int GetDamageStatus(int id, EVFDamageType type)
        {
            var v = BaseVehicle.Find(id); if (v == null) return 0;
            v.GetDamageStatus(out int p, out int d, out int l, out int t);
            return type switch { EVFDamageType.Panels => p, EVFDamageType.Doors => d, EVFDamageType.Lights => l, EVFDamageType.Tires => t, _ => 0 };
        }

        public static void UpdateDamageStatus(int id, EVFDamageType type, int val)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.GetDamageStatus(out int p, out int d, out int l, out int t);
            switch (type) { case EVFDamageType.Panels: p = val; break; case EVFDamageType.Doors: d = val; break; case EVFDamageType.Lights: l = val; break; case EVFDamageType.Tires: t = val; break; }
            v.UpdateDamageStatus(p, d, l, t);
        }

        public static void FullUpdateDamage(int id, EVFDamageType type)
        {
            int val = type switch { EVFDamageType.Panels => 0x03331111, EVFDamageType.Doors => 0x04040404, EVFDamageType.Lights => 0x0000000F, _ => 0 };
            UpdateDamageStatus(id, type, val);
        }

        // ── Panel Helpers ─────────────────────────────────────────────────
        public static void GetPanels(int id, out int fl, out int fr, out int rl, out int rr, out int wind, out int fb, out int rb)
        {
            var v = BaseVehicle.Find(id);
            fl = fr = rl = rr = wind = fb = rb = 0;
            if (v == null) return;
            v.GetDamageStatus(out int p, out _, out _, out _);
            fl = p & 0xF; fr = (p >> 4) & 0xF; rl = (p >> 8) & 0xF; rr = (p >> 12) & 0xF;
            wind = (p >> 16) & 0xF; fb = (p >> 20) & 0xF; rb = (p >> 24) & 0xF;
        }

        public static void SetPanels(int id, int fl, int fr, int rl, int rr, int wind, int fb, int rb)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.GetDamageStatus(out _, out int d, out int l, out int t);
            int p = fl | (fr << 4) | (rl << 8) | (rr << 12) | (wind << 16) | (fb << 20) | (rb << 24);
            v.UpdateDamageStatus(p, d, l, t);
        }

        public static void GetDoors(int id, out int bonnet, out int boot, out int driver, out int pass)
        {
            var v = BaseVehicle.Find(id);
            bonnet = boot = driver = pass = 0;
            if (v == null) return;
            v.GetDamageStatus(out _, out int d, out _, out _);
            bonnet = d & 0x7; boot = (d >> 8) & 0x7; driver = (d >> 16) & 0x7; pass = (d >> 24) & 0x7;
        }

        public static void SetDoors(int id, int bonnet, int boot, int driver, int pass)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.GetDamageStatus(out int p, out _, out int l, out int t);
            int d = bonnet | (boot << 8) | (driver << 16) | (pass << 24);
            v.UpdateDamageStatus(p, d, l, t);
        }

        public static void GetTires(int id, out int fl, out int fr, out int rl, out int rr)
        {
            var v = BaseVehicle.Find(id);
            fl = fr = rl = rr = 0;
            if (v == null) return;
            v.GetDamageStatus(out _, out _, out _, out int t);
            rr = t & 0x1; fr = (t >> 1) & 0x1; rl = (t >> 2) & 0x1; fl = (t >> 3) & 0x1;
        }

        public static void SetTires(int id, int fl, int fr, int rl, int rr)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.GetDamageStatus(out int p, out int d, out int l, out _);
            int t = rr | (fr << 1) | (rl << 2) | (fl << 3);
            v.UpdateDamageStatus(p, d, l, t);
        }

        public static void GetLights(int id, out int frontLeft, out int frontRight, out int back)
        {
            var v = BaseVehicle.Find(id);
            frontLeft = frontRight = back = 0;
            if (v == null) return;
            v.GetDamageStatus(out _, out _, out int l, out _);
            frontLeft = l & 0x1; frontRight = (l >> 2) & 0x1; back = (l >> 6) & 0x1;
        }

        public static void SetLights(int id, int frontLeft, int frontRight, int back)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.GetDamageStatus(out int p, out int d, out _, out int t);
            int l = frontLeft | (frontRight << 2) | (back << 6);
            v.UpdateDamageStatus(p, d, l, t);
        }

        // ── Teleport ──────────────────────────────────────────────────────
        public static void TeleportVehicle(int id, Vector3 pos, float angle, int worldId = -1, int interiorId = -1)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            foreach (var bp in BasePlayer.All)
            {
                if (bp.Vehicle?.Id == id)
                {
                    if (worldId >= 0) bp.VirtualWorld = worldId;
                    if (interiorId >= 0) bp.Interior = interiorId;
                }
            }
            if (worldId >= 0) v.VirtualWorld = worldId;
            if (interiorId >= 0) v.LinkToInterior(interiorId);
            v.Position = pos;
            if (_v.TryGetValue(id, out var d))
            {
                d.PosX = pos.X; d.PosY = pos.Y; d.PosZ = pos.Z; d.PosAngle = angle;
                if (worldId >= 0) d.SpawnWorld = worldId;
                if (interiorId >= 0) d.Interior = interiorId;
            }
            v.Angle = angle;
        }

        // ── Occupancy ─────────────────────────────────────────────────────
        public static bool IsVehicleOccupied(int id) => BasePlayer.All.Any(p => p.Vehicle?.Id == id);

        public static int GetDriver(int id)
        {
            foreach (var p in BasePlayer.All)
                if (p.State == PlayerState.Driving && p.Vehicle?.Id == id)
                    return p.Id;
            return -1;
        }

        public static bool IsValidVehicleDoor(int id, int doorId)
        {
            var v = BaseVehicle.Find(id); if (v == null) return false;
            return doorId <= GetModelDoors((int)v.Model);
        }

        // ── Speed ─────────────────────────────────────────────────────────
        public static float GetVehicleSpeed(int id)
        {
            var v = BaseVehicle.Find(id); if (v == null) return 0f;
            var vel = v.Velocity;
            return (float)Math.Sqrt(vel.X * vel.X + vel.Y * vel.Y + vel.Z * vel.Z) * EVFConstants.VehicleSpeedMultiplier;
        }

        // ── Model Info Lookups ────────────────────────────────────────────
        public static float GetTopSpeed(int modelId)
        {
            int i = modelId - 400;
            return i >= 0 && i < EVFConstants.TopSpeed.Length ? EVFConstants.TopSpeed[i] : 0f;
        }

        public static string GetVehicleName(int modelId)
        {
            int i = modelId - 400;
            return i >= 0 && i < EVFConstants.VehicleName.Length ? EVFConstants.VehicleName[i] : "Unknown";
        }

        public static int GetModelSeats(int modelId)
        {
            int i = modelId - 400;
            return i >= 0 && i < EVFConstants.VehicleSeats.Length ? EVFConstants.VehicleSeats[i] : 0;
        }

        public static int GetModelDoors(int modelId)
        {
            int i = modelId - 400;
            return i >= 0 && i < EVFConstants.VehicleDoors.Length ? EVFConstants.VehicleDoors[i] : 0;
        }

        public static bool GetRandomColors(int modelId, out int c1, out int c2)
        {
            c1 = c2 = 0;
            int i = modelId - 400;
            if (i < 0 || i >= EVFConstants.CarColors.Length) return false;
            var row = EVFConstants.CarColors[i];
            if (row[0] == 0) return false;
            if (row[0] == 2) { c1 = row[1]; c2 = row[2]; return true; }
            int r = _rng.Next(row[0]) & ~1;
            c1 = row[1 + r];
            c2 = row[2 + r];
            return true;
        }

        // ── Component ─────────────────────────────────────────────────────
        public static bool IsValidComponent(int vehicleId, int componentId)
        {
            var v = BaseVehicle.Find(vehicleId); if (v == null) return false;
            int modelId = (int)v.Model;
            if ((componentId >= 1008 && componentId <= 1010) || componentId == 1025 ||
                (componentId >= 1073 && componentId <= 1087) || (componentId >= 1096 && componentId <= 1098))
            {
                return modelId switch
                {
                    581 or 523 or 462 or 521 or 463 or 522 or 461 or 448 or 468 or 586 or
                    509 or 481 or 510 or 472 or 473 or 493 or 595 or 484 or 430 or 453 or
                    452 or 446 or 454 or 590 or 569 or 537 or 538 or 570 or 449 => false,
                    _ => true
                };
            }
            int idx = modelId - 400;
            if (idx < 0 || idx >= EVFConstants.ValidComponent.Length) return false;
            var row = EVFConstants.ValidComponent[idx];
            for (int i = 1; i < row.Length; i++)
                if (row[i] == componentId) return true;
            return false;
        }

        public static int GetComponentPrice(int componentId)
        {
            foreach (var pair in EVFConstants.ComponentPrice)
                if (pair[0] == componentId) return pair[1];
            return 0;
        }

        public static string GetComponentName(int componentId)
        {
            if (componentId < EVFConstants.MinComponentId || componentId > EVFConstants.MaxComponentId) return string.Empty;
            int i = componentId - EVFConstants.MinComponentId;
            return i < EVFConstants.ComponentNames.Length ? EVFConstants.ComponentNames[i] : string.Empty;
        }

        public static string GetComponentTypeName(int componentId)
        {
            int slotId = BaseVehicle.GetComponentType(componentId);
            return slotId >= 0 && slotId < EVFConstants.ComponentTypes.Length ? EVFConstants.ComponentTypes[slotId] : string.Empty;
        }

        public static bool IsTrailer(int vehicleId)
        {
            var v = BaseVehicle.Find(vehicleId); if (v == null) return false;
            return (int)v.Model is 435 or 450 or 584 or 591 or 606;
        }

        // ── Object Attachment  ────────────────────────────────────────────
        public static SampSharp.Streamer.World.DynamicObject GetVehicleSlotAttachedObject(int vehicleId, int slot)
        {
            var d = GetData(vehicleId); if (d == null) return null;
            if (slot < 0 || slot >= d.AttachedObjects.Length) return null;
            return d.AttachedObjects[slot];
        }

        public static bool IsObjectAttachedToVehicle(SampSharp.Streamer.World.DynamicObject obj, int vehicleId)
        {
            var d = GetData(vehicleId); if (d == null || obj == null) return false;
            return d.AttachedObjects.Contains(obj);
        }

        public static bool AttachObjectToVehicle(SampSharp.Streamer.World.DynamicObject obj, int vehicleId, Vector3 offset, Vector3 rot)
        {
            var d = GetData(vehicleId); if (d == null || obj == null) return false;
            for (int i = 0; i < d.AttachedObjects.Length; i++)
            {
                if (d.AttachedObjects[i] == null)
                {
                    obj.AttachTo(BaseVehicle.Find(vehicleId), offset, rot);
                    d.AttachedObjects[i] = obj;
                    return true;
                }
            }
            return false;
        }

        public static bool EditVehicle(int playerId, int vehicleId)
        {
            var d = GetData(vehicleId); if (d == null) return false;
            if (d.EditorObject != null) return false;
            var v = BaseVehicle.Find(vehicleId); if (v == null) return false;

            d.EditorObject = new SampSharp.Streamer.World.DynamicObject(19300, v.Position, new Vector3(0, 0, 0));
            d.EditorObject.Edit(BasePlayer.Find(playerId));

            var p = GetPlayerData(playerId);
            p.EditorVehicleId = vehicleId;
            p.EditorObject = null;
            return true;
        }

        public static bool EditVehicleObject(int playerId, int vehicleId, SampSharp.Streamer.World.DynamicObject obj)
        {
            var d = GetData(vehicleId); if (d == null || obj == null) return false;
            if (d.EditorObject != null) return false;
            var v = BaseVehicle.Find(vehicleId); if (v == null) return false;

            int model = obj.ModelId;
            obj.Dispose();

            d.EditorObject = new SampSharp.Streamer.World.DynamicObject(model, v.Position, new Vector3(0, 0, v.Angle));
            d.EditorObject.Edit(BasePlayer.Find(playerId));

            v.Angle = 0.0f;
            return true;
        }

        // ── Vehicle State Sets  ───────────────────────────────────────────
        public static void ResetVehicleProperties(int id)
        {
            var d = GetData(id); if (d == null) return;
            d.TrailerId = -1;
            d.Paintjob = 0;
            d.Interior = 0;
            d.Color1 = -1;
            d.Color2 = -1;
            d.Horn = 0;
            d.SpawnX = 0; d.SpawnY = 0; d.SpawnZ = 0; d.SpawnAngle = 0;
            d.SpawnWorld = 0; d.SpawnInterior = 0;
            d.SpeedCap = 0;
            d.FuelEnabled = false;
            d.Fuel = EVFConstants.DefaultVehicleFuel;
            d.Sticky = false;
            d.UnoccupiedDamage = false;
            d.Bomb = 0;
            d.Bulletproof = false;
            d.Stored = false;
        }

        // ── Nearest Vehicle ───────────────────────────────────────────────
        public static int GetNearestVehicleToPos(Vector3 pos, int worldId = -1, int interiorId = -1, float maxDist = 0f, int except = -1)
        {
            int found = -1;
            float best = -1f;
            foreach (var v in BaseVehicle.All)
            {
                if (v.Id == except) continue;
                if ((int)v.Model == 590) continue;
                if (worldId >= 0 && v.VirtualWorld != worldId) continue;
                if (interiorId >= 0 && GetVehicleInterior(v.Id) != interiorId) continue;
                float dist = v.Position.DistanceTo(pos);
                if (maxDist > 0f && dist > maxDist) continue;
                if (best < 0f || dist < best) { best = dist; found = v.Id; }
            }
            return found;
        }

        public static int GetNearestVehicleToPlayer(BasePlayer player, float maxDist = 0f)
            => GetNearestVehicleToPos(player.Position, player.VirtualWorld, player.Interior, maxDist, player.Vehicle?.Id ?? -1);

        // ── Event Handlers (call from GameMode/Player) ────────────────────
        public static void OnPlayerConnect(int playerId)
        {
            _p[playerId] = new EVFPlayerData();
        }

        public static void OnPlayerDisconnect(int playerId)
        {
            _p.Remove(playerId);
        }

        public static void OnVehicleSpawned(int vehicleId)
        {
            var v = BaseVehicle.Find(vehicleId); if (v == null) return;
            var d = GetData(vehicleId); if (d == null) return;

            v.ChangePaintjob(EVFConstants.ResetPaintjobId);
            d.Paintjob = EVFConstants.ResetPaintjobId;
            ChangeColor(vehicleId, d.Color1, d.Color2);
            d.Health = 1000f;

            if (d.SpawnX != 0 || d.SpawnY != 0 || d.SpawnZ != 0)
            {
                var cur = v.Position;
                float curA = v.Angle;
                if (cur.X != d.SpawnX || cur.Y != d.SpawnY || cur.Z != d.SpawnZ ||
                    curA != d.SpawnAngle || v.VirtualWorld != d.SpawnWorld || d.Interior != d.SpawnInterior)
                    TeleportVehicle(vehicleId, new Vector3(d.SpawnX, d.SpawnY, d.SpawnZ), d.SpawnAngle, d.SpawnWorld, d.SpawnInterior);
            }
        }

        public static void OnVehicleDied(int vehicleId)
        {
            var d = GetData(vehicleId);
            if (d != null)
            {
                for (int slot = 0; slot < 3; slot++)
                    SetVehicleNeonLights(vehicleId, false, 0, slot);
                DisableCarBlinking(vehicleId);
            }
        }

        public static void OnVehicleDamageStatusUpdate(int vehicleId)
        {
            if (GetData(vehicleId) == null) return;
            if (!IsDamageEnabled(vehicleId, EVFDamageType.Panels)) UpdateDamageStatus(vehicleId, EVFDamageType.Panels, 0);
            if (!IsDamageEnabled(vehicleId, EVFDamageType.Doors)) UpdateDamageStatus(vehicleId, EVFDamageType.Doors, 0);
            if (!IsDamageEnabled(vehicleId, EVFDamageType.Lights)) UpdateDamageStatus(vehicleId, EVFDamageType.Lights, 0);
            if (!IsDamageEnabled(vehicleId, EVFDamageType.Tires)) UpdateDamageStatus(vehicleId, EVFDamageType.Tires, 0);
        }

        public static void OnPlayerUpdate(int playerId)
        {
            var player = BasePlayer.Find(playerId) as BasePlayer; if (player == null) return;
            if (player.State != PlayerState.Driving) return;

            int vId = player.Vehicle?.Id ?? -1; if (vId < 0) return;
            var d = GetData(vId); if (d == null) return;

            if (d.SpeedCap > 0f)
            {
                float speed = GetVehicleSpeed(vId);
                if (speed > d.SpeedCap)
                {
                    PlayerReachedSpeedCap?.Invoke(null, new EVFSpeedCapEventArgs { PlayerId = playerId, VehicleId = vId, Speed = speed });
                    var v = BaseVehicle.Find(vId); if (v == null) return;
                    var vel = v.Velocity;
                    float factor = d.SpeedCap / speed;
                    v.Velocity = new Vector3(vel.X * factor, vel.Y * factor, vel.Z * factor);
                }
            }

            int trailer = player.Vehicle?.Trailer?.Id ?? -1;
            if (d.TrailerId != trailer)
            {
                if (trailer >= 0)
                {
                    TrailerHooked?.Invoke(null, new EVFTrailerEventArgs { PlayerId = playerId, VehicleId = vId, TrailerId = trailer });
                    if (IsCarBlinking(vId)) SetCarBlinking(vId, d.BlinkSide, true);
                }
                else if (d.TrailerId >= 0)
                {
                    TrailerUnhooked?.Invoke(null, new EVFTrailerEventArgs { PlayerId = playerId, VehicleId = vId, TrailerId = d.TrailerId });
                    if (IsCarBlinking(vId)) DisableCarBlinking(vId);
                }
                d.TrailerId = trailer;
            }
        }

        public static void OnPlayerStateChange(int playerId, PlayerState newState, PlayerState oldState)
        {
            var player = BasePlayer.Find(playerId); if (player == null) return;

            if (newState == PlayerState.Driving)
            {
                int vId = player.Vehicle?.Id ?? -1; if (vId < 0) return;
                var d = GetData(vId);
                if (d == null) return;
                if (IsBombed(vId) && !IsBombActivated(vId))
                    ActivateBomb(playerId, vId);
                if (IsSticky(vId))
                    SetParam(vId, EVFParamType.Engine, false);
            }
            if (oldState == PlayerState.Driving)
            {
                int vId = player.Vehicle?.Id ?? -1; if (vId < 0) return;
                if (IsSticky(vId))
                    SetParam(vId, EVFParamType.Engine, true);
            }
        }

        public static void OnEnterExitModShop(int playerId, bool entered, int interiorId)
        {
            GetPlayerData(playerId).InModShop = entered;
        }

        public static bool OnVehicleMod(int playerId, int vehicleId, int componentId)
        {
            var pd = GetPlayerData(playerId);
            if (!IsValidComponent(vehicleId, componentId) || !pd.InModShop)
                return false;
            return true;
        }

        public static void OnWeaponShot(int playerId, int weaponId, int hitType, int hitId, Vector3 hitPos)
        {
            if (hitType != (int)BulletHitType.Vehicle) return;
            var d = GetData(hitId); if (d == null) return;
            if (d.Bulletproof) return;

            var vehicle = BaseVehicle.Find(hitId); if (vehicle == null) return;
            int modelId = (int)vehicle.Model;
            float health = vehicle.Health;
            float dmg = weaponId < EVFConstants.WeaponDamage.Length ? EVFConstants.WeaponDamage[weaponId] : 1f;
            var bodyPart = EVFVehicleBodyPart.Unknown;

            if (health > 249f)
            {
                if (d.UnoccupiedDamage || IsVehicleOccupied(hitId))
                {
                    var modelType = (VehicleModelType)modelId;
                    int tireStatus = GetDamageStatus(hitId, EVFDamageType.Tires);

                    var fwVec = BaseVehicle.GetModelInfo(modelType, VehicleModelInfoType.WheelsFront);
                    float fwX = fwVec.X, fwY = fwVec.Y, fwZ = fwVec.Z;

                    if (VectorSize(hitPos.X + fwX, hitPos.Y - fwY, hitPos.Z - fwZ) <= 0.4f)
                    {
                        bodyPart = EVFVehicleBodyPart.FrontLeftWheel;
                        if (tireStatus + 8 > 15) UpdateDamageStatus(hitId, EVFDamageType.Tires, tireStatus + 8);
                    }
                    else if (VectorSize(hitPos.X - fwX, hitPos.Y - fwY, hitPos.Z - fwZ) <= 0.4f)
                    {
                        bodyPart = EVFVehicleBodyPart.FrontRightWheel;
                        if (tireStatus + 2 > 15) UpdateDamageStatus(hitId, EVFDamageType.Tires, tireStatus + 2);
                    }
                    else
                    {
                        var rwVec = BaseVehicle.GetModelInfo(modelType, VehicleModelInfoType.WheelsRear);
                        float rwX = rwVec.X, rwY = rwVec.Y, rwZ = rwVec.Z;

                        if (VectorSize(hitPos.X + rwX, hitPos.Y - rwY, hitPos.Z - rwZ) <= 0.4f)
                        {
                            bodyPart = EVFVehicleBodyPart.BackLeftWheel;
                            if (tireStatus + 4 > 15) UpdateDamageStatus(hitId, EVFDamageType.Tires, tireStatus + 4);
                        }
                        else if (VectorSize(hitPos.X - rwX, hitPos.Y - rwY, hitPos.Z - rwZ) <= 0.4f)
                        {
                            bodyPart = EVFVehicleBodyPart.BackRightWheel;
                            if (tireStatus + 1 > 15) UpdateDamageStatus(hitId, EVFDamageType.Tires, tireStatus + 1);
                        }
                        else
                        {
                            var pcVec = BaseVehicle.GetModelInfo(modelType, VehicleModelInfoType.PetrolCap);
                            float pcX = pcVec.X, pcY = pcVec.Y, pcZ = pcVec.Z;

                            if (VectorSize(hitPos.X - pcX, hitPos.Y - pcY, hitPos.Z - pcZ) <= 0.2f)
                            {
                                bodyPart = EVFVehicleBodyPart.PetrolCap;
                            }
                            else
                            {
                                health -= dmg;
                                vehicle.Health = health;
                                int hp = (int)health;
                                if (hp is >= 251 and <= 399) FullUpdateDamage(hitId, EVFDamageType.Doors);
                                else if (hp is >= 400 and <= 599) FullUpdateDamage(hitId, EVFDamageType.Panels);
                                else if (hp is >= 600 and <= 700) FullUpdateDamage(hitId, EVFDamageType.Lights);
                            }
                        }
                    }
                }
            }
            else
                vehicle.Respawn();

            PlayerShotVehicle?.Invoke(null, new EVFPlayerShotVehicleEventArgs
            {
                PlayerId = playerId,
                VehicleId = hitId,
                WeaponId = weaponId,
                Damage = dmg,
                BodyPart = bodyPart
            });
        }

        // ── Bomb Internal ─────────────────────────────────────────────────
        private static void ActivateBomb(int playerId, int vehicleId)
        {
            var d = GetData(vehicleId); if (d == null) return;
            BombActivated?.Invoke(null, new EVFBombEventArgs { PlayerId = playerId, VehicleId = vehicleId });
            SetParam(vehicleId, EVFParamType.Alarm, true);
            int interval = d.Bomb - 1750;
            if (interval < 1750)
            {
                d.BombTimer = new Timer(d.Bomb, false);
                d.BombTimer.Tick += (s, e) => { ExplodeBomb(playerId, vehicleId); d.BombTimer?.Dispose(); d.BombTimer = null; };
            }
            else
            {
                d.BombTimer = new Timer(interval, false);
                d.BombTimer.Tick += (s, e) =>
                {
                    d.BombTimer?.Dispose();
                    d.BombTimer = new Timer(1750, false);
                    d.BombTimer.Tick += (s2, e2) => { ExplodeBomb(playerId, vehicleId); d.BombTimer?.Dispose(); d.BombTimer = null; };
                };
            }
        }

        private static void ExplodeBomb(int playerId, int vehicleId)
        {
            SetParam(vehicleId, EVFParamType.Alarm, false);
            var d = GetData(vehicleId); if (d != null) { d.Bomb = 0; d.BombTimer = null; }
            var vehicle = BaseVehicle.Find(vehicleId);
            if (vehicle != null)
            {
                var pos = vehicle.Position;
                // CreateExplosionForAll(pos, ExplosionType.LargeVisibleDamage2, 30f);
                vehicle.Respawn();
            }
            BombExploded?.Invoke(null, new EVFBombEventArgs { PlayerId = playerId, VehicleId = vehicleId });
            var player = BasePlayer.Find(playerId);
            if (player != null && player.Vehicle?.Id == vehicleId)
            {
                player.Health = 0f;
                player.GameText("~r~Bombed !", 3500, 3);
            }
        }

        // ── Respray & Paint ─────────────────────────────────────────────────
        public static void OnVehicleRespray(int vehicleId, int c1, int c2)
        {
            var d = GetData(vehicleId);
            if (d == null) return;
            d.Color1 = c1;
            d.Color2 = c2;
        }

        public static void OnVehiclePaintjob(int vehicleId, int paintjobId)
        {
            var d = GetData(vehicleId);
            if (d == null) return;
            d.Paintjob = paintjobId;
        }

        public static (bool Valid, int Price) ValidateVehicleMod(int playerId, int vehicleId, int componentId)
        {
            var pd = GetPlayerData(playerId);
            var player = BasePlayer.Find(playerId);
            bool illegal = !IsValidComponent(vehicleId, componentId)
                        || !pd.InModShop
                        || player?.State == PlayerState.Passenger;  // tambah ini
            int price = GetComponentPrice(componentId);
            VehicleModEx?.Invoke(null, new EVFVehicleModExEventArgs
            {
                PlayerId = playerId,
                VehicleId = vehicleId,
                ComponentId = componentId,
                Price = price,
                Illegal = illegal
            });
            return (!illegal, price);
        }


        // ── Timer Update ──────────────────────────────────────────────────
        private static void UpdateVehicles()
        {
            foreach (var kv in _v)
            {
                var id = kv.Key; var d = kv.Value;
                if (!d.Stored) continue;

                var v = BaseVehicle.Find(id); if (v == null) continue;
                var pos = v.Position;
                var angle = v.Angle;

                if (d.Sticky)
                {
                    float dx = pos.X - d.PosX, dy = pos.Y - d.PosY, dz = pos.Z - d.PosZ;
                    float dist = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (dist >= 2f || Math.Abs(angle - d.PosAngle) >= 1f)
                    {
                        TeleportVehicle(id, new Vector3(d.PosX, d.PosY, d.PosZ), d.PosAngle);
                        continue;
                    }
                }

                float pdx = pos.X - d.PosX, pdy = pos.Y - d.PosY, pdz = pos.Z - d.PosZ;
                if ((float)Math.Sqrt(pdx * pdx + pdy * pdy + pdz * pdz) >= 2f)
                {
                    var posArgs = new EVFVehiclePosChangeEventArgs
                    {
                        VehicleId = id,
                        NewX = pos.X,
                        NewY = pos.Y,
                        NewZ = pos.Z,
                        NewAngle = angle,
                        OldX = d.PosX,
                        OldY = d.PosY,
                        OldZ = d.PosZ,
                        OldAngle = d.PosAngle
                    };
                    VehiclePosChanged?.Invoke(null, posArgs);
                    if (posArgs.Cancel)
                    {
                        TeleportVehicle(id, new Vector3(d.PosX, d.PosY, d.PosZ), d.PosAngle);
                        continue;
                    }
                    d.PosX = pos.X; d.PosY = pos.Y; d.PosZ = pos.Z; d.PosAngle = angle;
                }

                var vel = v.Velocity;
                if (vel.X != d.VelX || vel.Y != d.VelY || vel.Z != d.VelZ)
                {
                    var velArgs = new EVFVehicleVelocityChangeEventArgs
                    {
                        VehicleId = id,
                        NewX = vel.X,
                        NewY = vel.Y,
                        NewZ = vel.Z,
                        OldX = d.VelX,
                        OldY = d.VelY,
                        OldZ = d.VelZ
                    };
                    VehicleVelocityChanged?.Invoke(null, velArgs);
                    if (velArgs.Cancel)
                    {
                        v.Velocity = new Vector3(d.VelX, d.VelY, d.VelZ);
                        continue;
                    }
                    d.VelX = vel.X; d.VelY = vel.Y; d.VelZ = vel.Z;
                }

                if (d.FuelEnabled)
                {
                    float speed = GetVehicleSpeed(id);
                    if (d.Fuel <= 0)
                    {
                        SetParam(id, EVFParamType.Engine, false);
                        int driverId = GetDriver(id);
                        if (driverId >= 0)
                            BasePlayer.Find(driverId)?.GameText("~r~Vehicle out of fuel !", 5000, 3);
                    }
                    else
                    {
                        d.Uptime++;
                        if (d.Uptime > 20)
                        {
                            d.Uptime = 0;
                            int consumption = 0;
                            if (speed == 0f) { if (GetParam(id, EVFParamType.Engine)) consumption = EVFConstants.FuelMultiplier; }
                            else { for (int i = 20, top = (int)GetTopSpeed((int)v.Model); i <= top; i += 20) if (speed > i) consumption += EVFConstants.FuelMultiplier; }
                            if (consumption > 0)
                            {
                                int old = d.Fuel, newFuel = Math.Max(0, old - consumption - 1);
                                VehicleFuelChanged?.Invoke(null, new EVFVehicleFuelEventArgs { VehicleId = id, OldFuel = old, NewFuel = newFuel });
                                d.Fuel = newFuel;
                            }
                        }
                    }
                }

                float h = v.Health;
                if (Math.Abs(h - d.Health) > 0.01f)
                {
                    var hArgs = new EVFVehicleHealthChangeEventArgs
                    {
                        VehicleId = id,
                        NewHealth = h,
                        OldHealth = d.Health
                    };
                    VehicleHealthChanged?.Invoke(null, hArgs);
                    if (hArgs.Cancel)
                        v.Health = d.Health;
                    else
                        d.Health = h;
                }

                if (VehicleBlinking && GetParam(id, EVFParamType.Engine))
                {
                    if (Math.Abs(v.Angle - d.BlinkAngle) > 20.0f)
                    {
                        if (!IsCarBlinking(id))
                        {
                            SetCarBlinking(id, EVFBlinkSide.Emergency);
                            DisableCarBlinking(id);
                        }
                        else
                        {
                            DisableCarBlinking(id);
                        }
                    }
                }
                else if (!GetParam(id, EVFParamType.Engine))
                {
                    if (IsCarBlinking(id)) DisableCarBlinking(id);
                }
            }
        }

        private static float VectorSize(float x, float y, float z)
            => (float)Math.Sqrt(x * x + y * y + z * z);

        public static void SetVehicleHealth(int id, float health)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.Health = health;
            var d = GetData(id); if (d != null) d.Health = health;
        }

        public static void SetVehiclePosition(int id, Vector3 pos)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.Position = pos;
            var d = GetData(id); if (d != null) { d.PosX = pos.X; d.PosY = pos.Y; d.PosZ = pos.Z; }
        }

        public static void SetVehicleAngle(int id, float angle)
        {
            var v = BaseVehicle.Find(id); if (v == null) return;
            v.Angle = angle;
            var d = GetData(id); if (d != null) d.PosAngle = angle;
        }

        public static int GetVehicleSeats(int vehicleId)
        {
            var v = BaseVehicle.Find(vehicleId); if (v == null) return 0;
            return GetModelSeats((int)v.Model);
        }

        public static float GetVehicleTopSpeed(int vehicleId)
        {
            var v = BaseVehicle.Find(vehicleId); if (v == null) return 0f;
            return GetTopSpeed((int)v.Model);
        }

        public static int GetVehicleDoorCount(int vehicleId)
        {
            var v = BaseVehicle.Find(vehicleId); if (v == null) return 0;
            return GetModelDoors((int)v.Model);
        }

        public static string GetVehicleNameById(int vehicleId)
        {
            var v = BaseVehicle.Find(vehicleId); if (v == null) return "Unknown";
            return GetVehicleName((int)v.Model);
        }

        public static bool IsVehicleSeatOccupied(int vehicleId, int seatId)
            => BasePlayer.All.Any(p => p.Vehicle?.Id == vehicleId && p.VehicleSeat == seatId);

        public static int GetVehicleNextSeat(int vehicleId, int startSeat = 1)
        {
            int seats = GetVehicleSeats(vehicleId);
            for (int i = startSeat; i < seats; i++)
                if (!IsVehicleSeatOccupied(vehicleId, i)) return i;
            return -1;
        }

        public static int GetVehiclePassenger(int vehicleId)
        {
            var found = BasePlayer.All.FirstOrDefault(p =>
                p.State == PlayerState.Passenger && p.Vehicle?.Id == vehicleId);
            return found?.Id ?? -1;
        }

        public static float GetTopSpeed(VehicleModelType m) => GetTopSpeed((int)m);
        public static int GetModelSeats(VehicleModelType m) => GetModelSeats((int)m);
        public static int GetModelDoors(VehicleModelType m) => GetModelDoors((int)m);
        public static string GetVehicleName(VehicleModelType m) => GetVehicleName((int)m);
        public static bool GetRandomColors(VehicleModelType m, out int c1, out int c2)
            => GetRandomColors((int)m, out c1, out c2);
    }
}