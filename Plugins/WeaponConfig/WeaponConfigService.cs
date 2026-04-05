#nullable enable
using ProjectSMP.Plugins.SKY;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectSMP.Plugins.WeaponConfig
{
    internal sealed class PlayerWcState
    {
        public float Health = 100f, Armour = 0f;
        public float MaxHealth = 100f, MaxArmour = 100f;
        public bool IsDying, BeingResynced, TrueDeath = true, InClassSelection;
        public int Team = 255;
        public bool CbugAllowed = true;
        public int IntendedWorld;

        public int ShotsFired;
        public readonly int[] ShotTicks = new int[10];
        public readonly int[] ShotWeapons = new int[10];
        public int ShotIdx;

        public int HitsIssued;
        public readonly int[] HitTicks = new int[10];
        public readonly int[] HitWeapons = new int[10];
        public int HitIdx;

        public float LastDamageHealth;
        public float LastDamageArmour;
        public float LastZVelo;
        public float LastZ;
        public int LastUpdateTick;
        public int CbugFrozeTick;
        public int LastDeathTick;
        public int DeathSkipCount;
        public int LastDeathSkipTick;

        public ShotInfo? LastShot;
        public ResyncSnapshot? ResyncSnap;

        public const int MaxRejectedHits = 15;
        public readonly RejectedHit?[] RejectedHits = new RejectedHit?[MaxRejectedHits];
        public int RejectedHitIdx;

        public readonly PreviousHitInfo?[] PreviousHits = new PreviousHitInfo?[50];
        public int PreviousHitIdx;

        public CancellationTokenSource? DeathCts;
        public CancellationTokenSource? ResyncCts;

        public int SpawnClass = -2;
        public bool ForceClassSelection;
        public bool FirstSpawn = true;
        public int LastAnim = -1;
        public int LastStopTick;
        public int KnifeTimeout;
        public Weapon LastExplosive = Weapon.None;
        public int LastVehicleEnterTime;
        public int LastVehicleTick;

        public int LastSentHealth = -1;
        public int LastSentArmour = -1;

        public string DeathAnimLib = "PED";
        public string DeathAnimName = "FLOOR_HIT";
    }

    internal sealed class ShotInfo
    {
        public int Tick;
        public int WeaponId;
        public int HitType;
        public int HitId;
        public int Hits;
        public Vector3 Origin;
        public Vector3 HitPos;
        public float Length;
        public bool Valid;
    }

    internal sealed class VehicleState
    {
        public bool Alive;
        public int LastShooter = -1;
    }

    public static partial class WeaponConfigService
    {
        private const int DeathWorld = 0x00DEAD00;
        private static float _playerStreamDistance = 200f;
        private static float _maxDistFromShot = 5f;
        private static float _maxDistFromOrigin = 5f;
        private static int _shotTimeoutMs = 1000;
        private static int _deathSkipTimeout = 500;
        private static int _maxPreviousHits = 10;
        private const int BodypartTorso = 3;
        private const int BodypartHead = 9;

        private static WeaponConfig _cfg = new();
        private static WeaponEntry[] _weapons = Array.Empty<WeaponEntry>();
        private static readonly Dictionary<int, PlayerWcState> _states = new();
        private static readonly Dictionary<int, VehicleState> _vehicles = new();
        private static readonly Dictionary<int, SpawnClassInfo> _spawnClasses = new();
        private static readonly Dictionary<int, SpawnInfo> _playerSpawnInfo = new();
        private static readonly Dictionary<int, SpawnInfo> _playerFallbackSpawnInfo = new();

        public static event EventHandler<PlayerDamageArgs>? PlayerDamage;
        public static event EventHandler<PlayerDamageArgs>? PlayerDamageDone;
        public static event EventHandler<RejectedHitArgs>? RejectedHit;
        public static event EventHandler<PrepareDeathArgs>? PlayerPrepareDeath;
        public static event EventHandler<DeathFinishedArgs>? PlayerDeathFinished;
        public static event EventHandler<InvalidWeaponDamageArgs>? InvalidWeaponDamage;
        public static event EventHandler<VendingMachineArgs>? PlayerUseVendingMachine;

        private static readonly int[] _validTaken =
        {
            1,1,1,1,1,1,1,1,1,1,
            1,1,1,1,1,1,0,0,0,0,
            0,0,1,1,1,1,1,1,1,1,
            1,1,1,1,1,0,0,2,1,0,
            0,1,1,0,0,0,1,0,0,2,
            2,2,0,2,2
        };

        private static readonly bool[] _validGiven =
        {
            true,  true,  true,  true,  true,  true,  true,  true,  true,  true,
            true,  true,  true,  true,  true,  true,  false, false, false, false,
            false, false, true,  true,  true,  true,  true,  true,  true,  true,
            true,  true,  true,  true,  true,  false, false, false, true,  false,
            false, true,  true,  false, false, false, true,  false, false
        };

        public static void Init(WeaponConfig cfg, WeaponEntry[] weapons)
        {
            _cfg = cfg;
            _weapons = weapons;
            _playerStreamDistance = cfg.PlayerStreamDistance;
            _maxDistFromShot = cfg.MaxDistFromShot;
            _maxDistFromOrigin = cfg.MaxDistFromOrigin;
            _shotTimeoutMs = cfg.ShotTimeoutMs;
            _deathSkipTimeout = cfg.DeathSkipTimeout;
            _maxPreviousHits = cfg.MaxPreviousHits;
            WeaponConfigDamageFeed.Init(cfg.EnableDamageFeed, cfg.DamageFeedHideDelay);
            WeaponConfigHealthBar.Init();
            WeaponConfigVendingMachines.Init(cfg.CustomVendingMachines);

            WeaponConfigVendingMachines.PlayerUseVendingMachine += (s, e)
                => PlayerUseVendingMachine?.Invoke(s, e);
        }

        public static void Shutdown()
        {
            WeaponConfigHealthBar.Dispose();
            WeaponConfigVendingMachines.Dispose();
        }

        public static void OnConnect(Player p)
        {
            _states[p.Id] = new PlayerWcState { CbugAllowed = _cfg.CbugAllowed };
            WeaponConfigHealthBar.OnConnect(p);
            WeaponConfigDamageFeed.OnConnect(p);
            WeaponConfigVendingMachines.OnConnect(p);

            if (!_cfg.EnableHealthBar) WeaponConfigHealthBar.SetEnabled(p, false);
            if (!_cfg.EnableDamageFeed) WeaponConfigDamageFeed.SetEnabled(p, false);
        }

        public static void OnDisconnect(Player p)
        {
            if (_states.TryGetValue(p.Id, out var s))
            {
                s.DeathCts?.Cancel();
                s.ResyncCts?.Cancel();
            }
            _playerSpawnInfo.Remove(p.Id);
            _playerFallbackSpawnInfo.Remove(p.Id);
            _states.Remove(p.Id);

            foreach (var vs in _vehicles.Values)
                if (vs.LastShooter == p.Id)
                    vs.LastShooter = -1;

            WeaponConfigHealthBar.OnDisconnect(p);
            WeaponConfigDamageFeed.OnDisconnect(p);
            WeaponConfigVendingMachines.OnDisconnect(p);
        }

        public static void OnSpawn(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.DeathCts?.Cancel();

            var snap = s.ResyncSnap;
            s.ResyncSnap = null;

            if (snap != null)
            {
                s.Health = snap.Health;
                s.Armour = snap.Armour;
            }
            else
            {
                if (s.Health <= 0) s.Health = s.MaxHealth;
                if (s.Armour < 0) s.Armour = 0;
            }

            s.IsDying = false;
            s.BeingResynced = false;
            s.TrueDeath = true;
            s.InClassSelection = false;
            if (s.FirstSpawn)
            {
                s.FirstSpawn = false;
                WeaponConfigVendingMachines.OnFirstSpawn(p);
            }
            s.ShotsFired = 0;
            s.DeathSkipCount = 0;
            s.LastDeathSkipTick = 0;
            s.HitsIssued = 0;
            s.LastShot = null;
            s.IntendedWorld = p.VirtualWorld;
            p.Health = 99999f;
            p.Armour = 0f;
            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
            WeaponConfigHealthBar.Show(p);
        }

        public static void OnDeath(Player p, Player? killer, int reason)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            if (s.IsDying && !s.TrueDeath) return;

            var now = Environment.TickCount;
            if (now - s.LastDeathTick < _deathSkipTimeout)
            {
                s.DeathSkipCount++;
                s.LastDeathSkipTick = now;
                return;
            }

            s.IsDying = true;
            s.TrueDeath = false;
            s.LastDeathTick = now;
            s.IntendedWorld = p.VirtualWorld;
            if (_cfg.CbugDeathDelay && !s.CbugAllowed)
                s.CbugFrozeTick = Environment.TickCount;

            var (lib, anim) = GetDeathAnim(reason, 0);
            var prep = new PrepareDeathArgs
            {
                Player = p,
                AnimLib = lib,
                AnimName = anim,
                AnimLock = true,
                RespawnTime = _cfg.RespawnTime
            };
            PlayerPrepareDeath?.Invoke(null, prep);
            if (prep.Cancel) return;

            s.DeathAnimLib = prep.AnimLib;
            s.DeathAnimName = prep.AnimName;

            p.ApplyAnimation(prep.AnimLib, prep.AnimName, 4.0f, false, false, false, prep.AnimLock, 0, true);

            p.ToggleControllable(false);
            p.ResetWeapons();
            p.ApplyAnimation(prep.AnimLib, prep.AnimName, 4.0f, false, false, false, prep.AnimLock, 0, true);

            var cts = new CancellationTokenSource();
            s.DeathCts = cts;
            _ = RunDeathAsync(p, s, prep.AnimLib, prep.AnimName, prep.AnimLock, prep.RespawnTime, cts.Token, cancelable: false);
        }

        public static void OnUpdate(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.LastUpdateTick = Environment.TickCount;

            TrackLastAnimation(p, s);
            TrackVehicleState(p, s);
            UpdateKnifeTimeout(s);

            if (s.IsDying)
            {
                if (!p.IsNPC)
                {
                    if (p.Health < 99999f)
                        p.Health = 99999f;

                    if (p.SpecialAction != SpecialAction.None || p.AnimationIndex == 0)
                        p.ApplyAnimation(s.DeathAnimLib, s.DeathAnimName, 4.0f, false, false, false, true, 0, true);
                }
                return;
            }

            if (_cfg.CustomFallDamage && !s.IsDying) CheckFallDamage(p, s);
            if (_cfg.CustomVendingMachines) WeaponConfigVendingMachines.OnUpdate(p);
        }

        private static void TrackLastAnimation(Player p, PlayerWcState s)
        {
            var anim = p.AnimationIndex;
            if (anim != s.LastAnim)
            {
                s.LastAnim = anim;
                if (anim == 0) s.LastStopTick = Environment.TickCount;
            }
        }

        private static void TrackVehicleState(Player p, PlayerWcState s)
        {
            var inVehicle = p.Vehicle != null;
            var now = Environment.TickCount;
            if (inVehicle) s.LastVehicleTick = now;
            else if (s.LastVehicleTick > 0 && now - s.LastVehicleTick < 500)
                s.LastVehicleEnterTime = now;
        }

        private static void UpdateKnifeTimeout(PlayerWcState s)
        {
            if (s.KnifeTimeout > 0)
            {
                var elapsed = Environment.TickCount - s.LastUpdateTick;
                s.KnifeTimeout = Math.Max(0, s.KnifeTimeout - elapsed);
            }
        }

        public static void OnStartSpectating(Player spectator, Player target)
        {
            WeaponConfigDamageFeed.SetSpectating(spectator, target.Id);
            if (_cfg.CustomVendingMachines)
                WeaponConfigVendingMachines.OnStartSpectating(spectator);
        }

        public static void OnStopSpectating(Player spectator)
        {
            if (!_states.TryGetValue(spectator.Id, out var s)) return;
            s.DeathCts?.Cancel();
            s.IsDying = false;
            spectator.ToggleControllable(true);
            WeaponConfigDamageFeed.ClearSpectating(spectator);
        }

        internal static void CancelDeathForSpectating(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.DeathCts?.Cancel();
            s.IsDying = false;
            p.ToggleControllable(true);
            p.VirtualWorld = s.IntendedWorld;
        }

        public static void OnRequestClass(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.InClassSelection = true;
            if (s.ForceClassSelection)
            {
                s.ForceClassSelection = false;
                s.InClassSelection = true;
                return;
            }
            s.IsDying = false;
            s.DeathCts?.Cancel();
        }

        public static void HandleWeaponShot(Player p, int weaponId, int hitType, int hitId,
            Vector3 origin, Vector3 hitPos)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            if (weaponId == 16 || weaponId == 18 || weaponId == 35 || weaponId == 36 || weaponId == 39)
            {
                s.LastExplosive = (Weapon)weaponId;
            }

            s.LastShot = new ShotInfo
            {
                Tick = Environment.TickCount,
                WeaponId = weaponId,
                HitType = hitType,
                HitId = hitId,
                Hits = 0,
                Origin = origin,
                HitPos = hitPos,
                Length = Dist(origin, hitPos),
                Valid = hitType == 1 || !IsBulletWeapon(weaponId)
            };
            TrackShot(s, weaponId);

            if (hitType == 2 && hitId >= 0 && hitId < 2000)
            {
                if (!_vehicles.TryGetValue(hitId, out var vs))
                {
                    vs = new VehicleState { Alive = true };
                    _vehicles[hitId] = vs;
                }
                vs.LastShooter = p.Id;
            }
        }

        private static bool ApplyLagCompensation(Player issuer, PlayerWcState iState, Player damaged, PlayerWcState dState, int weaponId)
        {
            if (_cfg.LagCompensation == LagCompMode.Disabled) return true;

            var distToTarget = Dist(issuer.Position, damaged.Position);
            if (distToTarget > _playerStreamDistance)
            {
                Reject(issuer, weaponId, HitRejectReason.Unstreamed, distToTarget, targetName: damaged.Name);
                return false;
            }

            if (_cfg.LagCompensation == LagCompMode.Strict)
            {
                if (iState.LastShot == null) return false;

                var hitDist = Dist(iState.LastShot.Origin, damaged.Position);
                if (hitDist > _maxDistFromShot)
                {
                    Reject(issuer, weaponId, HitRejectReason.TooFarFromShot, hitDist, targetName: damaged.Name);
                    return false;
                }

                var originDist = Dist(iState.LastShot.Origin, issuer.Position);
                if (originDist > _maxDistFromOrigin)
                {
                    Reject(issuer, weaponId, HitRejectReason.TooFarFromOrigin, originDist, targetName: damaged.Name);
                    return false;
                }
            }

            return true;
        }

        public static void HandleGiveDamage(Player issuer, Player? damaged, float amount,
            int weaponId, int bodypart)
        {
            if (damaged == null || damaged.IsDisposed)
            {
                Reject(issuer, weaponId, HitRejectReason.Disconnected);
                return;
            }

            if (!_states.TryGetValue(issuer.Id, out var iState)) return;
            if (!_states.TryGetValue(damaged.Id, out var dState)) return;

            if (weaponId < 0 || weaponId >= _validGiven.Length || !_validGiven[weaponId])
            {
                FireInvalid(issuer, damaged, amount, weaponId, bodypart, 1, true);
                Reject(issuer, weaponId, HitRejectReason.InvalidWeapon, targetName: damaged.Name);
                return;
            }

            if (dState.IsDying)
            {
                Reject(issuer, weaponId, HitRejectReason.DyingPlayer, targetName: damaged.Name);
                return;
            }

            if (dState.BeingResynced)
            {
                Reject(issuer, weaponId, HitRejectReason.BeingResynced, targetName: damaged.Name);
                return;
            }

            if (!IsSpawned(damaged, dState))
            {
                Reject(issuer, weaponId, HitRejectReason.NotSpawned, targetName: damaged.Name);
                return;
            }

            if (iState.Team != 255 && iState.Team == dState.Team)
            {
                Reject(issuer, weaponId, HitRejectReason.SameTeam, targetName: damaged.Name);
                return;
            }

            var issuerVeh = issuer.Vehicle?.Id ?? -1;
            var damagedVeh = damaged.Vehicle?.Id ?? -1;

            if (issuerVeh != -1 && issuerVeh == damagedVeh)
            {
                Reject(issuer, weaponId, HitRejectReason.SameVehicle, targetName: damaged.Name);
                return;
            }

            if (issuerVeh != -1 && damagedVeh == -1 && !_cfg.VehiclePassengerDamage)
            {
                Reject(issuer, weaponId, HitRejectReason.OwnVehicle, targetName: damaged.Name);
                return;
            }

            if (!iState.CbugAllowed && issuer.Vehicle == null && Environment.TickCount - iState.LastVehicleEnterTime < 500)
            {
                Reject(issuer, weaponId, HitRejectReason.InvalidWeapon, targetName: damaged.Name);
                return;
            }

            if (!iState.CbugAllowed && iState.IsDying &&
                Environment.TickCount - iState.CbugFrozeTick < 500)
            {
                Reject(issuer, weaponId, HitRejectReason.InvalidWeapon, targetName: damaged.Name);
                return;
            }

            if (IsBulletWeapon(weaponId))
            {
                if (iState.LastShot == null || !iState.LastShot.Valid ||
                    Environment.TickCount - iState.LastShot.Tick > _shotTimeoutMs)
                {
                    Reject(issuer, weaponId, HitRejectReason.LastShotInvalid, targetName: damaged.Name);
                    return;
                }

                if (iState.LastShot.WeaponId != weaponId)
                {
                    iState.LastShot.Valid = false;
                    Reject(issuer, weaponId, HitRejectReason.LastShotInvalid, targetName: damaged.Name);
                    return;
                }

                if (iState.LastShot.HitType != -1 && iState.LastShot.HitType != 1)
                {
                    iState.LastShot.Valid = false;
                    Reject(issuer, weaponId, HitRejectReason.InvalidHitType,
                        iState.LastShot.HitType, targetName: damaged.Name);
                    return;
                }

                if (!ApplyLagCompensation(issuer, iState, damaged, dState, weaponId))
                    return;

                iState.LastShot.Hits++;

                if (IsShotgunWeapon(weaponId) && iState.LastShot.Hits > 1)
                {
                    iState.LastShot.Valid = false;
                    Reject(issuer, weaponId, HitRejectReason.MultiplePlayersShotgun,
                        iState.LastShot.Hits, targetName: damaged.Name);
                    return;
                }

                if (!IsShotgunWeapon(weaponId) && iState.LastShot.Hits > 1)
                {
                    iState.LastShot.Valid = false;
                    Reject(issuer, weaponId, HitRejectReason.MultiplePlayers,
                        iState.LastShot.Hits, targetName: damaged.Name);
                    return;
                }

                var w = GetEntry(weaponId);
                if (w.Range > 0)
                {
                    var dist = Dist(issuer.Position, damaged.Position);
                    if (dist > w.Range * 1.5f)
                    {
                        Reject(issuer, weaponId, HitRejectReason.OutOfRange,
                            dist, w.Range * 1.5f, targetName: damaged.Name);
                        return;
                    }
                }
            }

            if (!CheckShootRate(issuer, iState, weaponId)) return;
            if (!CheckHitRate(issuer, iState, weaponId)) return;

            TrackHit(iState, weaponId);

            var entry = GetEntry(weaponId);
            var actual = CalcDamage(entry, amount, issuer.Position, damaged.Position);
            if (actual < 0)
            {
                FireInvalid(issuer, damaged, amount, weaponId, bodypart, 3, true);
                Reject(issuer, weaponId, HitRejectReason.InvalidDamage, amount, targetName: damaged.Name);
                return;
            }

            TrackPreviousHit(dState, issuer.Id, weaponId, actual, bodypart);

            var args = new PlayerDamageArgs
            {
                Player = damaged,
                Issuer = issuer,
                Amount = actual,
                Weapon = weaponId,
                Bodypart = bodypart
            };
            PlayerDamage?.Invoke(null, args);
            if (args.Cancel) return;

            if (args.Player.Id != damaged.Id)
            {
                if (!_states.TryGetValue(args.Player.Id, out dState)) return;
                damaged = args.Player;
            }

            Inflict(damaged, dState, args.Amount, weaponId, bodypart, issuer);
            WeaponConfigDamageFeed.AddGiven(issuer, damaged.Name, args.Amount, weaponId);
            WeaponConfigDamageFeed.AddTaken(damaged, issuer.Name, args.Amount, weaponId);
            PlayerDamageDone?.Invoke(null, args);
            if (damaged.IsCharLoaded)
            {
                _ = DamageLogService.LogDamageAsync(
                    damaged.CitizenId,
                    issuer.CharInfo.Username,
                    weaponId,
                    args.Amount,
                    bodypart
                );
            }
        }

        public static void HandleVehicleDamage(Player issuer, int vehicleId, float amount, int weaponId)
        {
            if (!_states.TryGetValue(issuer.Id, out var iState)) return;
            if (iState.IsDying || !IsSpawned(issuer, iState)) return;

            var vehicle = BaseVehicle.Find(vehicleId);
            if (vehicle == null) return;

            if (!_cfg.VehicleUnoccupiedDamage && !IsVehicleOccupied(vehicle)) return;
            if (!_cfg.VehiclePassengerDamage && issuer.Vehicle?.Id == vehicleId) return;

            foreach (var bp in BasePlayer.All)
            {
                if (bp is not Player target || target.IsDisposed || target.Id == issuer.Id) continue;
                if (target.Vehicle?.Id != vehicleId) continue;
                if (!_states.TryGetValue(target.Id, out var tState) || tState.IsDying) continue;
                if (iState.Team != 255 && iState.Team == tState.Team) continue;

                var entry = GetEntry(weaponId);
                var actual = CalcDamage(entry, amount, issuer.Position, target.Position);
                if (actual <= 0) continue;

                TrackPreviousHit(tState, issuer.Id, weaponId, actual, 0);

                var args = new PlayerDamageArgs
                {
                    Player = target,
                    Issuer = issuer,
                    Amount = actual,
                    Weapon = weaponId,
                    Bodypart = 0
                };
                PlayerDamage?.Invoke(null, args);
                if (args.Cancel) continue;

                Inflict(target, tState, args.Amount, weaponId, 0, issuer);
                WeaponConfigDamageFeed.AddGiven(issuer, target.Name, args.Amount, weaponId);
                WeaponConfigDamageFeed.AddTaken(target, issuer.Name, args.Amount, weaponId);
                PlayerDamageDone?.Invoke(null, args);
                if (target.IsCharLoaded)
                {
                    _ = DamageLogService.LogDamageAsync(
                        target.CitizenId,
                        issuer.CharInfo.Username,
                        weaponId,
                        args.Amount,
                        0
                    );
                }
            }
        }

        public static void HandleTakeDamage(Player damaged, Player? issuer, float amount, int weaponId, int bodypart)
        {
            if (issuer != null && weaponId >= 0 && weaponId < _validGiven.Length
                && _validGiven[weaponId])
                return;

            if (!_states.TryGetValue(damaged.Id, out var s) || s.IsDying) return;

            if (_cfg.CustomFallDamage && weaponId == 54)
                return;

            if (weaponId < 0 || weaponId >= _validTaken.Length || _validTaken[weaponId] == 0)
            {
                FireInvalid(damaged, issuer, amount, weaponId, bodypart, 1, false);
                return;
            }

            var w = GetEntry(weaponId);
            var actual = w.Type == DamageType.Static ? w.Damage : amount * w.Damage;
            if (actual < 0) { FireInvalid(damaged, issuer, amount, weaponId, bodypart, 3, false); return; }

            if (issuer != null)
                TrackPreviousHit(s, issuer.Id, weaponId, actual, bodypart);

            var args = new PlayerDamageArgs
            {
                Player = damaged,
                Issuer = issuer,
                Amount = actual,
                Weapon = weaponId,
                Bodypart = bodypart
            };
            PlayerDamage?.Invoke(null, args);
            if (args.Cancel) return;

            if (args.Player.Id != damaged.Id)
            {
                if (!_states.TryGetValue(args.Player.Id, out s)) return;
                damaged = args.Player;
            }

            Inflict(damaged, s, args.Amount, weaponId, bodypart, issuer);
            if (issuer != null)
            {
                WeaponConfigDamageFeed.AddGiven(issuer, damaged.Name, args.Amount, weaponId);
                WeaponConfigDamageFeed.AddTaken(damaged, issuer.Name, args.Amount, weaponId);
            }
            else
            {
                WeaponConfigDamageFeed.AddTaken(damaged, GetWeaponName(weaponId), args.Amount, weaponId);
            }
            PlayerDamageDone?.Invoke(null, args);
            if (damaged.IsCharLoaded && issuer != null)
            {
                _ = DamageLogService.LogDamageAsync(
                    damaged.CitizenId,
                    issuer.CharInfo.Username,
                    weaponId,
                    args.Amount,
                    bodypart
                );
            }
        }

        private static void TrackPreviousHit(PlayerWcState s, int issuerId, int weapon, float amount, int bodypart)
        {
            var idx = s.PreviousHitIdx % _maxPreviousHits;
            s.PreviousHits[idx] = new PreviousHitInfo
            {
                Tick = Environment.TickCount,
                Issuer = issuerId,
                Weapon = weapon,
                Amount = amount,
                Health = s.Health,
                Armour = s.Armour,
                Bodypart = bodypart
            };
            s.PreviousHitIdx++;
        }

        public static PreviousHitInfo? GetPreviousHit(Player p, int index)
        {
            if (!_states.TryGetValue(p.Id, out var s) || index >= _maxPreviousHits)
                return null;

            var realIdx = ((s.PreviousHitIdx - index) % _maxPreviousHits + _maxPreviousHits) % _maxPreviousHits;
            return s.PreviousHits[realIdx];
        }

        private static void Inflict(Player p, PlayerWcState s, float amount, int weaponId,
            int bodypart, Player? issuer = null, bool ignoreArmour = false)
        {
            if (amount <= 0) return;
            var w = GetEntry(weaponId);

            var affectsArmour = !ignoreArmour && !_cfg.GlobalArmourRules && w.AffectsArmour;
            var torsoOnly = !_cfg.GlobalTorsoRules && w.TorsoOnly;

            float aDmg = 0, hDmg;
            if (affectsArmour && (!torsoOnly || bodypart == BodypartTorso))
            {
                aDmg = MathF.Min(amount, s.Armour);
                s.Armour -= aDmg;
                hDmg = amount - aDmg;
            }
            else hDmg = amount;

            s.LastDamageHealth = hDmg;
            s.LastDamageArmour = aDmg;
            s.Health = MathF.Max(0, s.Health - hDmg);

            p.Health = 99999f;
            p.Armour = 0f;

            if (hDmg > 0 || aDmg > 0)
            {
                p.PlaySound(_cfg.DamageTakenSound);
                issuer?.PlaySound(_cfg.DamageGivenSound);
            }

            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);

            if (s.Health <= 0 && !s.IsDying)
            {
                if (p.IsNPC)
                    WeaponConfigHealthBar.Update(p, 0f, s.MaxHealth);
                else
                    TriggerDeath(p, s, weaponId, bodypart, cancelable: true);
            }
        }

        private static void TriggerDeath(Player p, PlayerWcState s, int weaponId, int bodypart = 0, bool cancelable = true)
        {
            var now = Environment.TickCount;
            if (now - s.LastDeathTick < _deathSkipTimeout)
            {
                s.DeathSkipCount++;
                s.LastDeathSkipTick = now;
                return;
            }

            s.IsDying = true;
            s.TrueDeath = true;
            s.Armour = 0;
            s.LastDeathTick = now;
            s.IntendedWorld = p.VirtualWorld;
            if (_cfg.CbugDeathDelay && !s.CbugAllowed)
                s.CbugFrozeTick = Environment.TickCount;

            try { Plugins.SKY.SkyNatives.Instance.SendDeath(p.Id); } catch { }

            var (lib, anim) = GetDeathAnim(weaponId, bodypart);
            var prep = new PrepareDeathArgs
            {
                Player = p,
                AnimLib = lib,
                AnimName = anim,
                AnimLock = true,
                RespawnTime = _cfg.RespawnTime
            };
            PlayerPrepareDeath?.Invoke(null, prep);
            if (prep.Cancel) return;

            s.DeathAnimLib = prep.AnimLib;
            s.DeathAnimName = prep.AnimName;

            p.ToggleControllable(false);
            p.ResetWeapons();
            p.ApplyAnimation(prep.AnimLib, prep.AnimName, 4.0f, false, false, false, prep.AnimLock, 0, true);

            var cts = new CancellationTokenSource();
            s.DeathCts = cts;
            _ = RunDeathAsync(p, s, prep.AnimLib, prep.AnimName, prep.AnimLock, prep.RespawnTime, cts.Token, cancelable);
        }

        private static (string Lib, string Name) GetDeathAnim(int wid, int bodypart)
        {
            if (wid is 16 or 18 or 35 or 36 or 39 or 51)
                return ("PARACHUTE", "FALL_SKYDIVE_DIE");

            if (wid == 54)
                return ("PARACHUTE", "FALL_SKYDIVE_DIE");

            if (wid is 49 or 50 or 52)
                return ("PED", "BIKE_FALL_OFF");

            if (bodypart == BodypartHead)
                return ("PED", "KO_SHOT_FACE");

            if (wid is 25 or 26 or 27)
                return ("PED", "KO_SHOT_FRONT");

            return ("PED", "FLOOR_HIT");
        }

        private static void CheckFallDamage(Player p, PlayerWcState s)
        {
            var zVelo = p.Velocity.Z;
            if (zVelo < -0.1f)
                Console.WriteLine($"[FallDebug] zVelo={zVelo:F4} last={s.LastZVelo:F4} threshold={_cfg.FallDeathVelocity}");

            if (zVelo < _cfg.FallDeathVelocity && s.LastZVelo >= _cfg.FallDeathVelocity)
            {
                var amount = MathF.Abs(zVelo) * _cfg.FallDamageMultiplier;
                var args = new PlayerDamageArgs
                {
                    Player = p,
                    Issuer = null,
                    Amount = amount,
                    Weapon = 54,
                    Bodypart = 0
                };
                PlayerDamage?.Invoke(null, args);
                if (!args.Cancel)
                    Inflict(p, s, args.Amount, 54, 0);
                Console.WriteLine($"[FallDamage] zVelo={zVelo} amount={amount}");
            }
            s.LastZVelo = zVelo;
            s.LastZ = p.Position.Z;
        }

        private static bool CheckShootRate(Player p, PlayerWcState s, int weaponId)
        {
            var w = GetEntry(weaponId);
            if (w.MaxShootRate <= 0 || IsHighRateWeapon(weaponId) || s.ShotsFired < 2) return true;

            var n = Math.Min(s.ShotsFired, _cfg.MaxShootRateSamples);
            if (n < 2) return true;

            var total = 0;
            var multi = false;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.ShotIdx - i - 1 + s.ShotTicks.Length) % s.ShotTicks.Length;
                var bi = (s.ShotIdx - i - 2 + s.ShotTicks.Length) % s.ShotTicks.Length;
                total += s.ShotTicks[ai] - s.ShotTicks[bi];
                if (s.ShotWeapons[ai] != s.ShotWeapons[bi]) multi = true;
            }
            var avg = total / (n - 1);
            if (avg >= w.MaxShootRate) return true;

            Reject(p, weaponId,
                multi ? HitRejectReason.ShootRateTooFastMultiple : HitRejectReason.ShootRateTooFast,
                avg, n, multi ? 0 : w.MaxShootRate);
            return false;
        }

        private static bool CheckHitRate(Player p, PlayerWcState s, int weaponId)
        {
            var w = GetEntry(weaponId);
            if (w.MaxShootRate <= 0 || IsHighRateWeapon(weaponId) || s.HitsIssued < 2) return true;

            var n = Math.Min(s.HitsIssued, _cfg.MaxHitRateSamples);
            if (n < 2) return true;

            var total = 0;
            var multi = false;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.HitIdx - i - 1 + s.HitTicks.Length) % s.HitTicks.Length;
                var bi = (s.HitIdx - i - 2 + s.HitTicks.Length) % s.HitTicks.Length;
                total += s.HitTicks[ai] - s.HitTicks[bi];
                if (s.HitWeapons[ai] != s.HitWeapons[bi]) multi = true;
            }
            var avg = total / (n - 1);
            if (avg >= w.MaxShootRate) return true;

            Reject(p, weaponId,
                multi ? HitRejectReason.HitRateTooFastMultiple : HitRejectReason.HitRateTooFast,
                avg, n, multi ? 0 : w.MaxShootRate);
            return false;
        }

        private static void TrackShot(PlayerWcState s, int wid)
        {
            s.ShotTicks[s.ShotIdx] = Environment.TickCount;
            s.ShotWeapons[s.ShotIdx] = wid;
            s.ShotIdx = (s.ShotIdx + 1) % s.ShotTicks.Length;
            s.ShotsFired++;
        }

        private static void TrackHit(PlayerWcState s, int wid)
        {
            s.HitTicks[s.HitIdx] = Environment.TickCount;
            s.HitWeapons[s.HitIdx] = wid;
            s.HitIdx = (s.HitIdx + 1) % s.HitTicks.Length;
            s.HitsIssued++;
        }

        private static async Task RunDeathAsync(Player p, PlayerWcState s, string animLib, string animName, bool animLock, int delayMs, CancellationToken ct, bool cancelable)
        {
            try
            {
                p.VirtualWorld = DeathWorld;
                p.ApplyAnimation(animLib, animName, 4.0f, false, false, false, animLock, 0, true);

                var elapsed = 0;
                while (elapsed < delayMs)
                {
                    if (p.IsDisposed) return;

                    var animIndex = p.AnimationIndex;
                    if (animIndex > 0)
                    {
                        if (p.GetAnimationName(out var currentLib, out var currentName))
                        {
                            if (!string.Equals(currentLib, animLib, StringComparison.OrdinalIgnoreCase) ||
                                !string.Equals(currentName, animName, StringComparison.OrdinalIgnoreCase))
                            {
                                p.ApplyAnimation(animLib, animName, 4.0f, false, false, false, animLock, 0, true);
                            }
                        }
                    }
                    else
                    {
                        p.ApplyAnimation(animLib, animName, 4.0f, false, false, false, animLock, 0, true);
                    }

                    await Task.Delay(1000, ct);
                    elapsed += 1000;
                }

                if (p.IsDisposed) return;

                s.IsDying = false;
                s.Health = s.MaxHealth;
                s.Armour = 0;
                p.VirtualWorld = s.IntendedWorld;
                p.ToggleControllable(true);

                PlayerDeathFinished?.Invoke(null,
                    new DeathFinishedArgs { Player = p, Cancelable = cancelable });
            }
            catch (OperationCanceledException)
            {
                if (!p.IsDisposed)
                {
                    p.ToggleControllable(true);
                    p.VirtualWorld = s.IntendedWorld;
                    s.IsDying = false;
                }
            }
        }

        private static ResyncSnapshot CaptureSnapshot(Player p, PlayerWcState s)
        {
            var snap = new ResyncSnapshot
            {
                Health = s.Health,
                Armour = s.Armour,
                Skin = p.Skin,
                Team = s.Team,
                Position = p.Position,
                FacingAngle = p.Angle
            };
            for (var slot = 0; slot < 13; slot++)
            {
                p.GetWeaponData(slot, out var wep, out var ammo);
                snap.Weapons[slot] = (wep, ammo);
            }
            return snap;
        }

        private static void RestoreFromSnapshot(Player p, PlayerWcState s,
            ResyncSnapshot snap, bool restoreWeapons)
        {
            s.Health = snap.Health;
            s.Armour = snap.Armour;

            if (restoreWeapons)
            {
                p.ResetWeapons();
                foreach (var (wep, ammo) in snap.Weapons)
                    if (ammo > 0 && wep != Weapon.None)
                        p.GiveWeapon(wep, ammo);
            }

            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
        }

        public static void ResyncPlayer(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            try
            {
                Anticheat.Integration.WeaponConfigBridge.OnPlayerResync(p.Id);
            }
            catch { }

            s.ResyncCts?.Cancel();
            s.BeingResynced = true;
            s.ResyncSnap = CaptureSnapshot(p, s);
            p.Position = p.Position;
            try { SkyNatives.Instance.SpawnPlayerForWorld(p.Id); } catch { }
            try { SkyNatives.Instance.FreezeSyncData(p.Id, true); SkyNatives.Instance.SetKeySyncBlocked(p.Id, 1); } catch { }
            var cts = new CancellationTokenSource();
            s.ResyncCts = cts;
            _ = ClearResyncAsync(p, s, cts.Token);
        }

        private static async Task ClearResyncAsync(Player p, PlayerWcState s, CancellationToken ct)
        {
            try
            {
                await Task.Delay(500, ct);
                if (p.IsDisposed) return;

                s.BeingResynced = false;
                var snap = s.ResyncSnap;
                s.ResyncSnap = null;

                if (snap != null)
                {
                    p.Skin = snap.Skin;
                    p.Position = snap.Position;
                    p.Angle = snap.FacingAngle;
                    RestoreFromSnapshot(p, s, snap, restoreWeapons: true);
                    try { SkyNatives.Instance.SendLastSyncData(p.Id, -1, 0); } catch { }
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                if (!p.IsDisposed)
                {
                    try
                    {
                        SkyNatives.Instance.FreezeSyncData(p.Id, false);
                        SkyNatives.Instance.SetKeySyncBlocked(p.Id, 0);
                    }
                    catch { }
                }
            }
        }

        private static float CalcDamage(WeaponEntry w, float given, Vector3 a, Vector3 b)
        {
            return w.Type switch
            {
                DamageType.Static => w.Damage,
                DamageType.Multiplier => given * w.Damage,
                DamageType.Range => CalcRangeDamage(w, Dist(a, b)),
                DamageType.RangeMultiplier => CalcRangeDamage(w, Dist(a, b)) * given,
                _ => w.Damage
            };
        }

        private static float CalcRangeDamage(WeaponEntry w, float dist)
        {
            if (w.RangeSteps == null || w.RangeSteps.Count == 0)
                return w.Damage;

            var damage = w.Damage;
            foreach (var step in w.RangeSteps)
            {
                if (dist > step.Range)
                    damage = step.Damage;
                else
                    break;
            }
            return damage;
        }

        private static void Reject(Player p, int wid, HitRejectReason r,
            float i1 = 0, float i2 = 0, float i3 = 0, string targetName = "")
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;

            var now = DateTime.Now;
            s.RejectedHits[s.RejectedHitIdx % PlayerWcState.MaxRejectedHits] = new RejectedHit
            {
                Time = Environment.TickCount,
                Hour = now.Hour,
                Minute = now.Minute,
                Second = now.Second,
                Weapon = wid,
                Reason = r,
                Info1 = i1,
                Info2 = i2,
                Info3 = i3,
                TargetName = targetName
            };
            s.RejectedHitIdx++;

            RejectedHit?.Invoke(null, new RejectedHitArgs
            {
                Player = p,
                Weapon = wid,
                Reason = r,
                Info1 = i1,
                Info2 = i2,
                Info3 = i3,
                TargetName = targetName
            });
        }

        private static void FireInvalid(Player p, Player? damaged, float amt,
            int wid, int bp, int err, bool given)
            => InvalidWeaponDamage?.Invoke(null, new InvalidWeaponDamageArgs
            {
                Player = p,
                Damaged = damaged,
                Amount = amt,
                Weapon = wid,
                Bodypart = bp,
                Error = err,
                Given = given
            });

        private static void SyncFakeVitals(Player p, PlayerWcState s)
        {
            try
            {
                var h = (int)MathF.Round(MathF.Min(s.Health, 100f));
                var a = (int)MathF.Round(MathF.Min(s.Armour, 100f));
                if (h != s.LastSentHealth)
                {
                    SkyNatives.Instance.SetFakeHealth(p.Id, h);
                    s.LastSentHealth = h;
                }
                if (a != s.LastSentArmour)
                {
                    SkyNatives.Instance.SetFakeArmour(p.Id, a);
                    s.LastSentArmour = a;
                }
            }
            catch { }
        }

        private static bool IsVehicleOccupied(BaseVehicle vehicle)
        {
            foreach (var p in BasePlayer.All)
                if (p is Player pl && !pl.IsDisposed && pl.Vehicle?.Id == vehicle.Id)
                    return true;
            return false;
        }

        private static bool IsSpawned(Player p, PlayerWcState s)
        {
            if (s.IsDying || s.BeingResynced || s.InClassSelection) return false;
            var st = (int)p.State;
            return st >= 1 && st <= 6;
        }

        private static WeaponEntry GetEntry(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id] : new WeaponEntry { Id = id };

        private static float Dist(Vector3 a, Vector3 b)
        {
            var dx = a.X - b.X; var dy = a.Y - b.Y; var dz = a.Z - b.Z;
            return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public static int GetLastShotVehicleId(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return -1;
            if (s.LastShot == null || s.LastShot.HitType != 2) return -1;
            return s.LastShot.HitId;
        }

        public static bool IsBulletWeapon(int id) => (id >= 22 && id <= 34) || id == 38;
        public static bool IsMeleeWeapon(int id) => (id >= 0 && id <= 15) || id == 48;
        public static bool IsHighRateWeapon(int id) => id == 37 || id == 41 || id == 42 || id == 52 || id == 53;
        public static bool IsShotgunWeapon(int id) => id == 25 || id == 26 || id == 27;

        public static bool IsPlayerSpawned(Player p)
            => _states.TryGetValue(p.Id, out var s) && IsSpawned(p, s);

        public static int GetIntendedVirtualWorld(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.IntendedWorld : p.VirtualWorld;

        public static void SendWcDeathMessage(Player? killer, Player killee, int weapon)
        {
            var mapped = weapon switch
            {
                19 => 31,
                20 => 38,
                21 => 35,
                48 => 0,
                52 => 49,
                55 => 53,
                _ => weapon
            };
            BasePlayer.SendDeathMessageToAll(killer, killee, (Weapon)mapped);
        }

        public static void SetPlayerHealth(Player p, float health, float armour = -1f)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            if (armour >= 0f)
                s.Armour = MathF.Max(0, MathF.Min(armour, s.MaxArmour));
            s.Health = MathF.Max(0, MathF.Min(health, s.MaxHealth));
            p.Health = 99999f;
            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
            if (s.Health <= 0)
            {
                s.Armour = 0;
                SyncFakeVitals(p, s);
                if (p.IsNPC)
                {
                    WeaponConfigHealthBar.Update(p, 0f, s.MaxHealth);
                }
                else
                {
                    var args = new PlayerDamageArgs
                    {
                        Player = p,
                        Issuer = null,
                        Amount = 0f,
                        Weapon = 55,
                        Bodypart = 0
                    };
                    PlayerDamage?.Invoke(null, args);
                    if (!args.Cancel)
                    {
                        TriggerDeath(p, s, 55, 0, cancelable: true);
                        PlayerDamageDone?.Invoke(null, args);
                    }
                }
            }
        }

        public static void SetPlayerArmour(Player p, float armour)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.Armour = MathF.Max(0, MathF.Min(armour, s.MaxArmour));
            p.Armour = MathF.Min(s.Armour, 100f);
            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
        }

        public static void HealPlayer(Player p, float amount)
        {
            if (!_states.TryGetValue(p.Id, out var s) || s.IsDying) return;
            if (amount <= 0) return;
            s.Health = MathF.Min(s.Health + amount, s.MaxHealth);
            p.Health = 99999f;
            SyncFakeVitals(p, s);
            WeaponConfigHealthBar.Update(p, s.Health, s.MaxHealth);
        }

        public static float GetWcHealth(Player p) => _states.TryGetValue(p.Id, out var s) ? s.Health : 0f;
        public static float GetWcArmour(Player p) => _states.TryGetValue(p.Id, out var s) ? s.Armour : 0f;

        public static void DamagePlayer(Player p, float amount, Player? issuer = null, int weapon = 55, int bodypart = 0, bool ignoreArmour = false)
        {
            if (!_states.TryGetValue(p.Id, out var s) || p.IsDisposed) return;
            if (amount < 0) return;
            Inflict(p, s, amount, weapon, bodypart, issuer, ignoreArmour);
        }

        public static void SetDamageFeed(bool enable)
        {
            _cfg.EnableDamageFeed = enable;
            WeaponConfigDamageFeed.SetGlobalEnabled(enable);
        }

        public static bool IsDamageFeedActive(Player? p = null)
            => p != null ? WeaponConfigDamageFeed.IsEnabled(p) : _cfg.EnableDamageFeed;

        public static void SetWeaponDamage(int id, float dmg, DamageType type = DamageType.Static)
        {
            if (id >= 0 && id < _weapons.Length)
            { _weapons[id].Damage = dmg; _weapons[id].Type = type; }
        }
        public static float GetWeaponDamage(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].Damage : 0f;

        public static void SetWeaponMaxRange(int id, float range)
        { if (id >= 0 && id < _weapons.Length && IsBulletWeapon(id)) _weapons[id].Range = range; }
        public static float GetWeaponMaxRange(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].Range : 0f;

        public static void SetWeaponShootRate(int id, int rate)
        { if (id >= 0 && id < _weapons.Length) _weapons[id].MaxShootRate = rate; }
        public static int GetWeaponShootRate(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id].MaxShootRate : 0;

        public static void SetWeaponName(int id, string name)
        { if (id >= 0 && id < _weapons.Length) _weapons[id].Name = name; }
        public static string GetWeaponName(int id)
        {
            if (id == 55) return "Unknown";
            if (id == 56) return "Unknown";
            return id >= 0 && id < _weapons.Length ? _weapons[id].Name : $"Weapon {id}";
        }

        public static void SetWeaponArmourRule(int id, bool affectsArmour, bool torsoOnly = false)
        {
            if (id >= 0 && id < _weapons.Length)
            { _weapons[id].AffectsArmour = affectsArmour; _weapons[id].TorsoOnly = torsoOnly; }
        }

        public static void SetCustomArmourRules(bool armourRules, bool torsoRules = false)
        {
            _cfg.GlobalArmourRules = armourRules;
            _cfg.GlobalTorsoRules = torsoRules;
        }

        public static WeaponEntry? GetWeaponEntry(int id)
            => id >= 0 && id < _weapons.Length ? _weapons[id] : null;

        public static void SetPlayerMaxHealth(Player p, float v)
        { if (_states.TryGetValue(p.Id, out var s)) s.MaxHealth = v; }
        public static void SetPlayerMaxArmour(Player p, float v)
        { if (_states.TryGetValue(p.Id, out var s)) s.MaxArmour = v; }
        public static float GetPlayerMaxHealth(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.MaxHealth : 0f;
        public static float GetPlayerMaxArmour(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.MaxArmour : 0f;

        public static float GetLastDamageHealth(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.LastDamageHealth : 0f;
        public static float GetLastDamageArmour(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s.LastDamageArmour : 0f;

        public static bool IsPlayerDying(Player p)
            => _states.TryGetValue(p.Id, out var s) && s.IsDying;
        public static bool IsPlayerPaused(Player p)
            => _states.TryGetValue(p.Id, out var s)
            && Environment.TickCount - s.LastUpdateTick > 2000;

        public static void SetPlayerTeam(Player p, int team)
        {
            if (_states.TryGetValue(p.Id, out var s)) { s.Team = team; p.Team = team; }
        }

        public static void SetCbugAllowed(bool allowed, Player? p = null)
        {
            if (p != null)
            {
                if (_states.TryGetValue(p.Id, out var s)) s.CbugAllowed = allowed;
                return;
            }
            _cfg.CbugAllowed = allowed;
            foreach (var s in _states.Values) s.CbugAllowed = allowed;
        }
        public static bool GetCbugAllowed(Player? p = null)
        {
            if (p != null) return _states.TryGetValue(p.Id, out var s) && s.CbugAllowed;
            return _cfg.CbugAllowed;
        }
        public static void SetCbugDeathDelay(bool v) => _cfg.CbugDeathDelay = v;

        public static void SetVehiclePassengerDamage(bool v) => _cfg.VehiclePassengerDamage = v;
        public static void SetVehicleUnoccupiedDamage(bool v) => _cfg.VehicleUnoccupiedDamage = v;

        public static void SetCustomFallDamage(bool toggle, float mult = 25f, float deathVel = -0.6f)
        {
            _cfg.CustomFallDamage = toggle;
            _cfg.FallDamageMultiplier = mult;
            _cfg.FallDeathVelocity = -MathF.Abs(deathVel);
            if (toggle && 54 < _weapons.Length) _weapons[54].Damage = mult;
        }

        public static void SetCustomVendingMachines(bool enable)
        {
            _cfg.CustomVendingMachines = enable;
            WeaponConfigVendingMachines.Dispose();
            WeaponConfigVendingMachines.Init(enable);
            if (enable)
                foreach (var bp in BasePlayer.All)
                    if (bp is Player p)
                        WeaponConfigVendingMachines.OnConnect(p);
        }

        public static void SetRespawnTime(int ms) => _cfg.RespawnTime = Math.Max(0, ms);
        public static int GetRespawnTime() => _cfg.RespawnTime;

        public static void SetDamageSounds(int taken, int given)
        {
            _cfg.DamageTakenSound = taken;
            _cfg.DamageGivenSound = given;
        }

        public static void SetLagCompMode(LagCompMode mode) => _cfg.LagCompensation = mode;
        public static LagCompMode GetLagCompMode() => _cfg.LagCompensation;
        public static void SetMaxShootRateSamples(int samples) => _cfg.MaxShootRateSamples = Math.Max(1, Math.Min(samples, 10));
        public static int GetMaxShootRateSamples() => _cfg.MaxShootRateSamples;
        public static void SetMaxHitRateSamples(int samples) => _cfg.MaxHitRateSamples = Math.Max(1, Math.Min(samples, 10));
        public static int GetMaxHitRateSamples() => _cfg.MaxHitRateSamples;

        public static void EnableHealthBarForPlayer(Player p, bool enable)
            => WeaponConfigHealthBar.SetEnabled(p, enable);
        public static void SetDamageFeedForPlayer(Player p, bool enable)
            => WeaponConfigDamageFeed.SetEnabled(p, enable);

        public static int AverageShootRate(Player p, int shots)
            => AverageShootRate(p, shots, out _);

        public static int AverageShootRate(Player p, int shots, out bool multipleWeapons)
        {
            multipleWeapons = false;
            if (!_states.TryGetValue(p.Id, out var s) || s.ShotsFired < shots) return -1;

            var n = Math.Min(shots, s.ShotTicks.Length);
            if (n < 2) return 1;

            var total = 0;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.ShotIdx - i - 1 + s.ShotTicks.Length) % s.ShotTicks.Length;
                var bi = (s.ShotIdx - i - 2 + s.ShotTicks.Length) % s.ShotTicks.Length;
                total += s.ShotTicks[ai] - s.ShotTicks[bi];
                if (s.ShotWeapons[ai] != s.ShotWeapons[bi]) multipleWeapons = true;
            }
            return total / (n - 1);
        }

        public static int AverageHitRate(Player p, int hits)
            => AverageHitRate(p, hits, out _);

        public static int AverageHitRate(Player p, int hits, out bool multipleWeapons)
        {
            multipleWeapons = false;
            if (!_states.TryGetValue(p.Id, out var s) || s.HitsIssued < hits) return -1;

            var n = Math.Min(hits, s.HitTicks.Length);
            if (n < 2) return 1;

            var total = 0;
            for (var i = 0; i < n - 1; i++)
            {
                var ai = (s.HitIdx - i - 1 + s.HitTicks.Length) % s.HitTicks.Length;
                var bi = (s.HitIdx - i - 2 + s.HitTicks.Length) % s.HitTicks.Length;
                total += s.HitTicks[ai] - s.HitTicks[bi];
                if (s.HitWeapons[ai] != s.HitWeapons[bi]) multipleWeapons = true;
            }
            return total / (n - 1);
        }

        public static RejectedHit? GetRejectedHit(Player p, int idx)
        {
            if (!_states.TryGetValue(p.Id, out var s) || idx >= PlayerWcState.MaxRejectedHits)
                return null;
            var realIdx = ((s.RejectedHitIdx - idx) % PlayerWcState.MaxRejectedHits + PlayerWcState.MaxRejectedHits) % PlayerWcState.MaxRejectedHits;
            var hit = s.RejectedHits[realIdx];
            if (hit == null || hit.Time == 0) return null;
            return hit;
        }

        public static void OnVehicleSpawn(int vehicleId)
        {
            if (!_vehicles.TryGetValue(vehicleId, out var vs))
            {
                vs = new VehicleState { Alive = true };
                _vehicles[vehicleId] = vs;
            }
            vs.Alive = true;
            vs.LastShooter = -1;
        }

        public static void OnVehicleDeath(int vehicleId)
        {
            if (!_vehicles.TryGetValue(vehicleId, out var vs)) return;
            vs.Alive = false;
        }

        public static void OnVehicleDestroy(int vehicleId)
        {
            _vehicles.Remove(vehicleId);
        }

        public static int AddPlayerClass(int skin, Vector3 pos, float angle,
            Weapon w1 = Weapon.None, int a1 = 0, Weapon w2 = Weapon.None, int a2 = 0,
            Weapon w3 = Weapon.None, int a3 = 0)
        {
            var id = _spawnClasses.Count;
            _spawnClasses[id] = new SpawnClassInfo
            {
                Skin = skin,
                Team = 0x7FFFFFFF,
                Position = pos,
                Rotation = angle,
                Weapon1 = w1,
                Ammo1 = a1,
                Weapon2 = w2,
                Ammo2 = a2,
                Weapon3 = w3,
                Ammo3 = a3
            };
            return id;
        }

        public static int AddPlayerClassEx(int team, int skin, Vector3 pos, float angle,
            Weapon w1 = Weapon.None, int a1 = 0, Weapon w2 = Weapon.None, int a2 = 0,
            Weapon w3 = Weapon.None, int a3 = 0)
        {
            var id = _spawnClasses.Count;
            _spawnClasses[id] = new SpawnClassInfo
            {
                Skin = skin,
                Team = team,
                Position = pos,
                Rotation = angle,
                Weapon1 = w1,
                Ammo1 = a1,
                Weapon2 = w2,
                Ammo2 = a2,
                Weapon3 = w3,
                Ammo3 = a3
            };
            return id;
        }

        public static SpawnClassInfo? GetSpawnClass(int classId)
            => _spawnClasses.TryGetValue(classId, out var c) ? c : null;

        public static SpawnInfo? GetPlayerSpawnInfo(Player p)
            => _playerSpawnInfo.TryGetValue(p.Id, out var info) ? info : null;

        public static void SetPlayerSpawnInfo(Player p, SpawnInfo info)
        {
            _playerSpawnInfo[p.Id] = info;
            if (_states.TryGetValue(p.Id, out var s))
                s.SpawnClass = -1;
        }

        public static SpawnInfo? GetPlayerFallbackSpawnInfo(Player p)
            => _playerFallbackSpawnInfo.TryGetValue(p.Id, out var info) ? info : null;

        public static void SetPlayerFallbackSpawnInfo(Player p, SpawnInfo info)
            => _playerFallbackSpawnInfo[p.Id] = info;

        public static string GetRejectedHitFormatted(Player p, int idx)
        {
            var hit = GetRejectedHit(p, idx);
            if (hit == null) return string.Empty;

            var weaponName = GetWeaponName((int)hit.Weapon);
            var targetName = hit.TargetName;
            var reason = FormatRejectedReason(hit.Reason, hit.Info1, hit.Info2, hit.Info3);

            return $"[{hit.Hour:D2}:{hit.Minute:D2}:{hit.Second:D2}] ({weaponName} -> {targetName}) {reason}";
        }

        private static string FormatRejectedReason(HitRejectReason reason, float i1, float i2, float i3)
        {
            return reason switch
            {
                HitRejectReason.ShootRateTooFast =>
                    $"Shooting rate too fast: {i1:F0} ({i2:F0} samples, max {i3:F0})",
                HitRejectReason.HitRateTooFast =>
                    $"Hit rate too fast: {i1:F0} ({i2:F0} samples, max {i3:F0})",
                HitRejectReason.ShootRateTooFastMultiple =>
                    $"Shooting rate too fast: {i1:F0} ({i2:F0} samples, multiple weapons)",
                HitRejectReason.HitRateTooFastMultiple =>
                    $"Hit rate too fast: {i1:F0} ({i2:F0} samples, multiple weapons)",
                HitRejectReason.OutOfRange =>
                    $"Hit out of range ({i1:F1} > {i2:F1})",
                HitRejectReason.MultiplePlayers =>
                    $"One bullet hit {i1:F0} players",
                HitRejectReason.MultiplePlayersShotgun =>
                    $"Hit too many players with shotgun: {i1:F0}",
                HitRejectReason.InvalidHitType =>
                    $"Invalid hit type: {i1:F0}",
                HitRejectReason.TooFarFromShot =>
                    $"Hit player too far from hit position (dist {i1:F1})",
                HitRejectReason.TooFarFromOrigin =>
                    $"Damage inflicted too far from current position (dist {i1:F1})",
                HitRejectReason.InvalidDamage =>
                    $"Invalid weapon damage ({i1:F4})",
                HitRejectReason.InvalidVehicle =>
                    $"Hit invalid vehicle: {i1:F0}",
                HitRejectReason.Disconnected =>
                    $"Hit a disconnected player ID: {i1:F0}",
                HitRejectReason.NoIssuer => "None or invalid player shot",
                HitRejectReason.InvalidWeapon => "Invalid weapon",
                HitRejectReason.LastShotInvalid => "Last shot invalid",
                HitRejectReason.DyingPlayer => "Hit a dying player",
                HitRejectReason.SameTeam => "Hit a teammate",
                HitRejectReason.Unstreamed => "Hit someone that can't see you (not streamed in)",
                HitRejectReason.BeingResynced => "Hit while being resynced",
                HitRejectReason.NotSpawned => "Hit when not spawned or dying",
                HitRejectReason.SameVehicle => "Hit a player in the same vehicle",
                HitRejectReason.OwnVehicle => "Hit the vehicle you're in",
                _ => "Unknown reject reason"
            };
        }

        public static string GetPreviousHitFormatted(Player p, int index)
        {
            var hit = GetPreviousHit(p, index);
            if (hit == null) return string.Empty;

            var issuerName = BasePlayer.Find(hit.Issuer)?.Name ?? "Unknown";
            var weaponName = GetWeaponName(hit.Weapon);
            var bodypartName = GetBodypartName(hit.Bodypart);

            return $"[{hit.Tick}] {issuerName} -> {hit.Amount:F1} dmg ({weaponName}, {bodypartName}) | HP: {hit.Health:F1}, Armour: {hit.Armour:F1}";
        }

        public static string GetBodypartName(int bodypart)
        {
            return bodypart switch
            {
                3 => "Torso",
                4 => "Groin",
                5 => "Left Arm",
                6 => "Right Arm",
                7 => "Left Leg",
                8 => "Right Leg",
                9 => "Head",
                _ => "Unknown"
            };
        }

        public static void SetPlayerStreamDistance(float distance)
        {
            _playerStreamDistance = Math.Max(0f, distance);
        }

        public static float GetPlayerStreamDistance() => _playerStreamDistance;

        public static void SetMaxDistFromShot(float distance)
        {
            _maxDistFromShot = Math.Max(0f, distance);
        }

        public static float GetMaxDistFromShot() => _maxDistFromShot;

        public static void SetMaxDistFromOrigin(float distance)
        {
            _maxDistFromOrigin = Math.Max(0f, distance);
        }

        public static float GetMaxDistFromOrigin() => _maxDistFromOrigin;

        public static void SetShotTimeout(int ms)
        {
            _shotTimeoutMs = Math.Max(0, ms);
        }

        public static int GetShotTimeout() => _shotTimeoutMs;

        public static void SetDeathSkipTimeout(int ms)
        {
            _deathSkipTimeout = Math.Max(0, ms);
        }

        public static int GetDeathSkipTimeout() => _deathSkipTimeout;

        public static void SetMaxPreviousHits(int count)
        {
            _maxPreviousHits = Math.Max(1, Math.Min(count, 50));
        }

        public static int GetMaxPreviousHits() => _maxPreviousHits;

        public static void SetWeaponRangeDamage(int weaponId, DamageType damageType, params float[] values)
        {
            if (weaponId < 0 || weaponId >= _weapons.Length) return;
            if (!IsBulletWeapon(weaponId)) return;
            if (damageType != DamageType.Range && damageType != DamageType.RangeMultiplier) return;
            if (values.Length == 0 || values.Length % 2 != 1) return;

            var weapon = _weapons[weaponId];
            weapon.Type = damageType;
            weapon.RangeSteps.Clear();

            weapon.Damage = values[0];

            for (int i = 1; i < values.Length; i += 2)
            {
                weapon.RangeSteps.Add(new RangeDamageStep
                {
                    Range = values[i],
                    Damage = values[i + 1]
                });
            }
        }

        public static WeaponEntry? GetWeaponEntryPublic(int weaponId)
            => GetWeaponEntry(weaponId);

        public static void ModifyWeaponEntry(int weaponId, Action<WeaponEntry> modifier)
        {
            var entry = GetWeaponEntry(weaponId);
            if (entry != null) modifier(entry);
        }

        public static void ForcePlayerClassSelection(Player p)
        {
            if (!_states.TryGetValue(p.Id, out var s)) return;
            s.ForceClassSelection = true;
            s.InClassSelection = true;
            p.ToggleSpectating(true);
        }

        public static bool IsPlayerInClassSelection(Player p)
            => _states.TryGetValue(p.Id, out var s) && s.InClassSelection;

        public static void SetDisableSyncBugs(bool toggle)
        {
            try
            {
                Plugins.SKY.SkyNatives.Instance.SetDisableSyncBugs(toggle ? 1 : 0);
            }
            catch { }
        }

        public static void SetKnifeSync(bool toggle)
        {
            try
            {
                Plugins.SKY.SkyNatives.Instance.SetKnifeSync(toggle ? 1 : 0);
            }
            catch { }
        }

        internal static PlayerWcState? GetPlayerState(Player p)
            => _states.TryGetValue(p.Id, out var s) ? s : null;
    }
}