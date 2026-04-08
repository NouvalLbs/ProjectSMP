#nullable enable
using ProjectSMP.Core;
using ProjectSMP.Entities;
using ProjectSMP.Entities.Players.Account;
using ProjectSMP.Entities.Players.Administrator;
using ProjectSMP.Entities.Players.Character;
using ProjectSMP.Entities.Players.Condition;
using ProjectSMP.Entities.Players.Inventory;
using ProjectSMP.Entities.Players.NameTag;
using ProjectSMP.Entities.Players.Needs;
using ProjectSMP.Entities.Vehicles.Engine;
using ProjectSMP.Entities.Vehicles.Seatbelt;
using ProjectSMP.Entities.Vehicles.Speedo;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Bank;
using ProjectSMP.Features.Bank.DynamicATM;
using ProjectSMP.Features.Bank.DynamicBank;
using ProjectSMP.Features.Chat;
using ProjectSMP.Features.CinematicCamera;
using ProjectSMP.Features.Dynamic.DynamicDoor;
using ProjectSMP.Features.EnterExit;
using ProjectSMP.Features.Jobs.Core.DynamicJob;
using ProjectSMP.Features.Jobs.Side.Bus;
using ProjectSMP.Features.Jobs.Side.Forklifter;
using ProjectSMP.Features.Jobs.Side.Sweeper;
using ProjectSMP.Features.Jobs.Side.Trashmaster;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Features.ProgressBar;
using ProjectSMP.Features.ProgressBar.Data;
using ProjectSMP.Plugins.EVF2;
using ProjectSMP.Plugins.RealtimeClock;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.Pools;
using SampSharp.GameMode.World;
using System;
using System.Threading.Tasks;

namespace ProjectSMP
{
    [PooledType]
    public partial class Player : BasePlayer
    {
        public override void OnConnected(EventArgs e)
        {
            base.OnConnected(e);
            ClientManager.CheckPlayerClient(this);
            WeaponConfigService.OnConnect(this);
            WeaponConfigService.PlayerDamage += OnPlayerDamage;
            WeaponConfigService.PlayerDeathFinished += OnDeathFinished;
            EVFService.OnPlayerConnect(Id);
            CinematicCameraService.Start(this);
            UserControlService.InitAsync(this);
            RealtimeClockService.OnPlayerConnect(Id);
            ChatService.Initialize(this);
            ProgressBarData = new ProgressBarData();
        }

        public override void OnDisconnected(DisconnectEventArgs e)
        {
            WeaponConfigService.PlayerDeathFinished -= OnDeathFinished;
            WeaponConfigService.PlayerDamage -= OnPlayerDamage;
            EVFService.OnPlayerDisconnect(Id);
            CinematicCameraService.Stop(this);
            EnterExitService.Cleanup(this);
            _ = SaveOnDisconnectAsync();
            UserControlService.Cleanup(this);
            CharacterService.Cleanup(this);
            WeaponConfigService.OnDisconnect(this);
            RealtimeClockService.OnPlayerDisconnect(Id);
            NeedsService.OnPlayerDisconnect(this);
            ConditionService.UnregisterPlayer(this);
            NameTagService.Cleanup(this);
            ChatService.Cleanup(this);
            AskService.ClearPlayerAsks(this);
            AdminVehicleService.Cleanup(this);
            _ = InventoryService.SaveAsync(this);
            ProgressBarService.OnPlayerDisconnect(this);
            SpeedometerService.OnPlayerDisconnect(this);
            SeatbeltService.OnPlayerDisconnect(this);
            ForklifterService.OnPlayerDisconnect(this);
            SweeperService.OnPlayerDisconnect(this);
            BusService.OnPlayerDisconnect(this);
            TrashmasterService.OnPlayerDisconnect(this);
            this.ClearPlayerData();
            base.OnDisconnected(e);
        }

        private async Task SaveOnDisconnectAsync()
        {
            try { await CharacterService.SaveAsync(this); }
            catch (Exception ex) { Console.WriteLine($"[Player] Save failed for {Name}: {ex.Message}"); }
        }

        public override void OnSpawned(SpawnEventArgs e)
        {
            base.OnSpawned(e);
            WeaponConfigService.OnSpawn(this);
            CharacterService.HandleSpawn(this);
            if (IsCharLoaded)
            {
                _ = BankService.LoadAsync(this);
                InventoryService.Initialize(this);
            }
        }

        public override void OnDeath(DeathEventArgs e)
        {
            base.OnDeath(e);
            if (IsCharLoaded)
            {
                var p = Position;
                CharSpawnPos = new CharPosition
                {
                    X = p.X,
                    Y = p.Y,
                    Z = p.Z,
                    A = Angle,
                    Interior = Interior,
                    World = this.GetVirtualWorldSafe()
                };
            }
            WeaponConfigService.OnDeath(this, e.Killer as Player, (int)e.DeathReason);
        }

        public override void OnRequestClass(RequestClassEventArgs e)
        {
            base.OnRequestClass(e);
            WeaponConfigService.OnRequestClass(this);
            if (!IsCharLoaded || WeaponConfigService.IsPlayerInClassSelection(this)) return;

            SetSpawnInfo(0, CharInfo.Skin, new Vector3(CharSpawnPos.X, CharSpawnPos.Y, CharSpawnPos.Z), CharSpawnPos.A);
            Spawn();
        }

        public override void OnUpdate(PlayerUpdateEventArgs e)
        {
            base.OnUpdate(e);
            WeaponConfigService.OnUpdate(this);
            EVFService.OnPlayerUpdate(Id);
        }

        private void OnDeathFinished(object? sender, DeathFinishedArgs e)
        {
            if (e.Player != this || IsDisposed) return;
            CharacterService.RespawnCharacter(this);
        }

        private void OnPlayerDamage(object? sender, PlayerDamageArgs e)
        {
            if (e.Player != this) return;
            // e.Player    → yang kena damage
            // e.Issuer    → yang nyerang (nullable)
            // e.Amount    → damage amount (bisa dimodifikasi)
            // e.Weapon    → weapon id
            // e.Bodypart  → bodypart
            // e.Cancel = true → batalkan damage
        }

        public override void OnGiveDamage(DamageEventArgs e)
        {
            if (e.OtherPlayer == null)
            {
                var vid = WeaponConfigService.GetLastShotVehicleId(this);
                if (vid >= 0)
                    WeaponConfigService.HandleVehicleDamage(this, vid, e.Amount, (int)e.Weapon);
            }
            else
            {
                WeaponConfigService.HandleGiveDamage(this, e.OtherPlayer as Player, e.Amount, (int)e.Weapon, (int)e.BodyPart);
            }
            base.OnGiveDamage(e);
        }

        public override void OnTakeDamage(DamageEventArgs e)
        {
            WeaponConfigService.HandleTakeDamage(this, e.OtherPlayer as Player, e.Amount, (int)e.Weapon, (int)e.BodyPart);
            base.OnTakeDamage(e);
        }

        public override void OnWeaponShot(WeaponShotEventArgs e)
        {
            WeaponConfigService.HandleWeaponShot(this, (int)e.Weapon, (int)e.BulletHitType, e.HitId, Position, e.Position);
            if (WeaponConfigService.IsBulletWeapon((int)e.Weapon))
                e.PreventDamage = true;

            EVFService.OnWeaponShot(Id, (int)e.Weapon, (int)e.BulletHitType, e.HitId, e.Position);
            base.OnWeaponShot(e);
        }

        public override void OnClickTextDraw(ClickTextDrawEventArgs e)
        {
            base.OnClickTextDraw(e);
            PreviewModelDialog.HandleClick(this, e.TextDraw);
        }

        public override void OnClickPlayerTextDraw(ClickPlayerTextDrawEventArgs e)
        {
            base.OnClickPlayerTextDraw(e);
            PreviewModelDialog.HandlePlayerTextDrawClick(this, e.PlayerTextDraw);
        }

        public override void OnCancelClickTextDraw(PlayerEventArgs e)
        {
            PreviewModelDialog.HandleCancel(this);
            base.OnCancelClickTextDraw(e);
        }

        public override void OnText(TextEventArgs e)
        {
            e.SendToPlayers = false;
            if (!IsCharLoaded)
            {
                base.OnText(e);
                return;
            }
            ChatService.ProcessChatText(this, e.Text);
            base.OnText(e);
        }

        public override void OnEnterVehicle(EnterVehicleEventArgs e)
        {
            base.OnEnterVehicle(e);
            ForklifterService.OnPlayerEnterVehicle(this, e.Vehicle as Vehicle, e.IsPassenger);
            SweeperService.OnPlayerEnterVehicle(this, e.Vehicle as Vehicle, e.IsPassenger);
            BusService.OnPlayerEnterVehicle(this, e.Vehicle as Vehicle, e.IsPassenger);
            TrashmasterService.OnPlayerEnterVehicle(this, e.Vehicle as Vehicle, e.IsPassenger);
        }

        public override void OnExitVehicle(PlayerVehicleEventArgs e)
        {
            base.OnExitVehicle(e);
            SeatbeltService.OnPlayerExitVehicle(this);
            ForklifterService.OnPlayerExitVehicle(this, e.Vehicle as Vehicle);
            SweeperService.OnPlayerExitVehicle(this, e.Vehicle as Vehicle);
            BusService.OnPlayerExitVehicle(this, e.Vehicle as Vehicle);
            TrashmasterService.OnPlayerExitVehicle(this, e.Vehicle as Vehicle);
        }

        public override void OnKeyStateChanged(KeyStateChangedEventArgs e)
        {
            base.OnKeyStateChanged(e);
            if (e.NewKeys.HasFlag(Keys.Yes) && !e.OldKeys.HasFlag(Keys.Yes)) {
                if (!TrashmasterService.HandleYKey(this))
                    EngineService.ToggleEngine(this);
            }

            if (e.NewKeys.HasFlag(Keys.Crouch) && !e.OldKeys.HasFlag(Keys.Crouch))
                EVFExtensions.HandleHorn(this);

            if (e.NewKeys.HasFlag(Keys.SecondaryAttack))
            {
                var doorId = DoorService.CheckPlayerInDoor(this, out bool isOutside);
                if (doorId != -1)
                {
                    if (!DoorService.GetDoor(doorId).IsGarage)
                    {
                        DoorService.HandleDoorKeyPress(this);
                        return;
                    }
                }

                BankPickupService.HandleInteract(this);
                ATMService.HandleInteract(this);
                JobPickupService.HandleInteract(this);
                TrashmasterService.HandleDropoutInteract(this);
            }

            if (e.NewKeys.HasFlag(Keys.Walk))
            {
                var doorId = DoorService.CheckPlayerInDoor(this, out bool isOutside);
                if (doorId != -1)
                {
                    if (DoorService.GetDoor(doorId).IsGarage)
                    {
                        DoorService.HandleDoorKeyPress(this);
                        return;
                    }
                }
            }
        }

        public override void OnStateChanged(StateEventArgs e)
        {
            base.OnStateChanged(e);
            EVFService.OnPlayerStateChange(Id, e.NewState, e.OldState);
            EngineService.OnPlayerStateChanged(this, e.NewState);
            SpeedometerService.OnPlayerStateChanged(this, e.NewState, e.OldState);
            SeatbeltService.OnPlayerStateChanged(this, e.NewState);
            ForklifterService.OnPlayerStateChanged(this, e.NewState, e.OldState);
            SweeperService.OnPlayerStateChanged(this, e.NewState, e.OldState);
            BusService.OnPlayerStateChanged(this, e.NewState, e.OldState);
            TrashmasterService.OnPlayerStateChanged(this, e.NewState, e.OldState);
        }

        public override void OnClickMap(PositionEventArgs e)
        {
            base.OnClickMap(e);
            WaypointTeleportService.HandleWaypoint(this, e.Position);
        }
    }
}