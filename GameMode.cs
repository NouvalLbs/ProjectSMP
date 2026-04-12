using ProjectSMP.Core;
using ProjectSMP.Core.Discords;
using ProjectSMP.Entities.Players.Administrator;
using ProjectSMP.Entities.Players.Condition;
using ProjectSMP.Entities.Players.Delay;
using ProjectSMP.Entities.Players.Needs;
using ProjectSMP.Entities.Vehicles.Handbrake;
using ProjectSMP.Entities.Vehicles.Impact;
using ProjectSMP.Entities.Vehicles.Seatbelt;
using ProjectSMP.Entities.Vehicles.Speedo;
using ProjectSMP.Extensions;
using ProjectSMP.Features.Bank.DynamicATM;
using ProjectSMP.Features.Bank.DynamicBank;
using ProjectSMP.Features.Bank.Paycheck;
using ProjectSMP.Features.Drunk;
using ProjectSMP.Features.Dynamic.DynamicDoor;
using ProjectSMP.Features.Dynamic.DynamicPickups;
using ProjectSMP.Features.Jobs.Core;
using ProjectSMP.Features.Jobs.Core.DynamicJob;
using ProjectSMP.Features.Jobs.Side.Bus;
using ProjectSMP.Features.Jobs.Side.Forklifter;
using ProjectSMP.Features.Jobs.Side.Sweeper;
using ProjectSMP.Features.Jobs.Side.Trashmaster;
using ProjectSMP.Features.Jobs.Side.Trashmaster.DynamicTrash;
using ProjectSMP.Features.LevelSystem;
using ProjectSMP.Features.PreviewModelDialog;
using ProjectSMP.Features.ProgressBar;
using ProjectSMP.Plugins.Anticheat;
using ProjectSMP.Plugins.Anticheat.Configuration;
using ProjectSMP.Plugins.CEF;
using ProjectSMP.Plugins.ColAndreas;
using ProjectSMP.Plugins.EVF2;
using ProjectSMP.Plugins.GarageBlocker;
using ProjectSMP.Plugins.RakNet;
using ProjectSMP.Plugins.RealtimeClock;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.Core.Callbacks;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Events;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Threading.Tasks;

namespace ProjectSMP
{
    public class GameMode : BaseMode
    {
        private AnticheatPlugin _anticheat = null!;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Initialize RakNet
            RakNetEventHandler.Initialize();
            ColAndreasService.Init();

            CefService.OnInitialized += (playerId, success) =>
            {
                if (!success) return;
                var player = BasePlayer.Find(playerId) as Player;
                if (player == null) return;

                CefService.CreateBrowser(playerId, browserId: 1, url: "http://localhost/#/", hidden: false, focused: false);
            };

            CefService.OnBrowserCreated += (playerId, browserId, statusCode) =>
            {
                Console.WriteLine($"[CEF] Browser created: player={playerId} browser={browserId} status={statusCode}");
                if (statusCode == 200 && browserId == 1)
                    CefService.AlwaysListenKeys(playerId, browserId, true);
            };

            CefEventHandler.Initialize();

            // Initialize Discord C#
            Task.Run(async () =>
            {
                try
                {
                    await DiscordService.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[+] Discord - Init failed: {ex.Message}");
                }
            });

            // Initialize GeoLocation
            GeoLocationService.Initialize();

            // Initialize Weapon Config
            var (wcCfg, wcWeapons) = WeaponConfigLoader.Load();
            WeaponConfigService.Init(wcCfg, wcWeapons);
            WeaponConfigHealthBar.Init();

            // Initialize Anticheat
            _anticheat = AnticheatPlugin.Create(configPath: "scriptfiles/AntiCheat.json", weaponConfigMode: true);
            _anticheat.RegisterEvents(this);
            _anticheat.Warnings.PunishmentRequired += OnAnticheatPunishment;

            // Initialize EVF2
            EVFService.Initialize();

            // Initialize Safe Extensions
            SafeServerExtensions.Initialize(_anticheat);
            SafeVehicleExtensions.Initialize(_anticheat);
            SafeNativeExtensions.Initialize(_anticheat);

            // Initialize Primary Config
            ConfigManager.Load();
            ConfigManager.ApplyGameConfig(this);

            // Initialize Localization
            LocalizationManager.Load();

            // Initialize PreviewModelDialog
            PreviewModelDialog.Init();

            // Initialize TextDrawManager
            TextDrawManager.Init();

            // Initialize Garage Blocker
            GarageBlockerService.Init();

            // Initialize Database Manager
            Task.Run(DatabaseManager.InitAsync).GetAwaiter().GetResult();

            // Initialize RealtimeClock
            RealtimeClockService.Init();
            RealtimeClockService.SetInterval(10000, restartTimer: false);
            RealtimeClockService.Sync(serverTime: true);

            // Initialize Needs Service
            NeedsService.Initialize();

            // Initialize Speedometer Service
            SpeedometerService.Initialize();

            // Initialize Handbrake Service
            HandbrakeService.Initialize();
            SeatbeltService.Initialize();
            DrunkManager.Initialize();

            // Initialize Vehicle Impact Service
            VehicleImpactService.Initialize();

            // Initialize Condition Service
            ConditionService.Initialize();

            // Initialize Jail Service
            JailService.Initialize();

            // Initialize Playing Time Service
            PlaytimeService.Initialize();

            // Initialize Delay Service
            DelayService.Initialize();

            // Initialize Paycheck Service
            PaycheckService.Initialize();

            // Initialize Report Service
            ReportService.Initialize();

            // Initialize Ask Service
            AskService.Initialize();

            // Initialize Progressbar
            ProgressBarService.Initialize();

            // Initialize SideJobs - Forklifter
            ForklifterService.Initialize();
            SweeperService.Initialize();
            BusService.Initialize();
            TrashmasterService.Initialize();

            // Initialize Dynamic Pickups
            PickupService.Initialize();
            var pickupDataList = Task.Run(PickupService.LoadDataAsync).GetAwaiter().GetResult();
            PickupService.CreatePickupObjects(pickupDataList);

            // Initialize Dynamic Doors
            DoorService.Initialize();
            var doorDataList = Task.Run(DoorService.LoadDataAsync).GetAwaiter().GetResult();
            DoorService.CreateDoorObjects(doorDataList);

            // Initialize Dynamic Banks
            BankPickupService.Initialize();
            var bankDataList = Task.Run(BankPickupService.LoadDataAsync).GetAwaiter().GetResult();
            BankPickupService.CreateObjects(bankDataList);

            // Initialize Dynamic ATMs
            ATMService.Initialize();
            var atmDataList = Task.Run(ATMService.LoadDataAsync).GetAwaiter().GetResult();
            ATMService.CreateObjects(atmDataList);

            // Initialize Dynamic Jobs
            JobPickupService.Initialize();
            var jobDataList = Task.Run(JobPickupService.LoadDataAsync).GetAwaiter().GetResult();
            JobPickupService.CreateObjects(jobDataList);

            // Initialize Dynamic Trashes
            TrashService.Initialize();
            var trashDataList = Task.Run(TrashService.LoadDataAsync).GetAwaiter().GetResult();
            TrashService.CreateObjects(trashDataList);
        }

        private void OnAnticheatPunishment(int playerId, string checkName, PunishAction action)
        {
            var player = BasePlayer.Find(playerId);
            if (player is null) return;

            string message = $"{{FF0000}}[ANTICHEAT] {checkName}";

            switch (action)
            {
                case PunishAction.Kick:
                    player.SendClientMessage(Color.Red, message);
                    Console.WriteLine($"[AC-KICK] {player.Name} (ID:{playerId}) - {checkName}");

                    // Delay kick agar message terkirim
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        BasePlayer.Find(playerId)?.Kick();
                    });
                    break;

                case PunishAction.Ban:
                    player.SendClientMessage(Color.Red, message);
                    Console.WriteLine($"[AC-BAN] {player.Name} (ID:{playerId}) - {checkName}");

                    // Delay ban agar message terkirim
                    Task.Delay(100).ContinueWith(_ =>
                    {
                        BasePlayer.Find(playerId)?.Ban();
                    });
                    break;
            }
        }

        protected override void OnPlayerCommandText(BasePlayer player, CommandTextEventArgs e)
        {
            if (player is Player p && !p.IsCharLoaded)
            {
                e.Success = true;
                return;
            }

            base.OnPlayerCommandText(player, e);

            if (!e.Success && player is Player p2)
            {
                p2.SendClientMessage(Color.White, $"{{b9b9b9}}Command '{e.Text}' tidak ada, gunakan '/help'.");
                e.Success = true;
            }
        }

        protected override void OnVehicleSpawned(BaseVehicle vehicle, EventArgs e)
        {
            base.OnVehicleSpawned(vehicle, e);
            WeaponConfigService.OnVehicleSpawn(vehicle.Id);
            EVFService.OnVehicleSpawned(vehicle.Id);
        }

        protected override void OnVehicleDied(BaseVehicle vehicle, PlayerEventArgs e)
        {
            base.OnVehicleDied(vehicle, e);
            WeaponConfigService.OnVehicleDeath(vehicle.Id);
            EVFService.OnVehicleDied(vehicle.Id);
        }

        protected override void OnVehicleDamageStatusUpdated(BaseVehicle vehicle, PlayerEventArgs e)
        {
            base.OnVehicleDamageStatusUpdated(vehicle, e);
            EVFService.OnVehicleDamageStatusUpdate(vehicle.Id);
        }

        protected override void OnPlayerEnterExitModShop(BasePlayer player, EnterModShopEventArgs e)
        {
            base.OnPlayerEnterExitModShop(player, e);
            EVFService.OnEnterExitModShop(player.Id, e.EnterExit == EnterExit.Entered, e.InteriorId);
        }

        protected override void OnExited(EventArgs e)
        {
            WeaponConfigHealthBar.Dispose();
            EVFService.Dispose();
            PreviewModelDialog.Dispose();
            RealtimeClockService.Dispose();
            NeedsService.Dispose();
            SpeedometerService.Dispose();
            HandbrakeService.Dispose();
            SeatbeltService.Dispose();
            DrunkManager.Dispose();
            VehicleImpactService.Dispose();
            ConditionService.Dispose();
            JailService.Dispose();
            PlaytimeService.Dispose();
            DelayService.Dispose();
            PaycheckService.Dispose();
            ReportService.Dispose();
            AskService.Dispose();
            ProgressBarService.Dispose();
            ForklifterService.Dispose();
            SweeperService.Dispose();
            SideJobVehicleManager.Dispose();
            BusService.Dispose();
            TrashmasterService.Dispose();
            GeoLocationService.Dispose();
            TrashService.Dispose();

            try
            {
                DiscordService.ShutdownAsync().GetAwaiter().GetResult();
                DiscordEventBus.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[+] Discord - Shutdown error: {ex.Message}");
            }

            base.OnExited(e);
        }

        [Callback]
        public void OnCefInitializeCS(int player_id, int success)
            => CefService.HandleCefInitialize(player_id, success);

        [Callback]
        public void OnCefBrowserCreatedCS(int player_id, int browser_id, int status_code)
            => CefService.HandleBrowserCreated(player_id, browser_id, status_code);

        [Callback]
        public void OnCefClientEventCS(int player_id, string args_json)
            => CefService.HandleClientEvent(player_id, args_json);

        [Callback]
        public void RNB_OnIncomingPacket(int playerId, int packetId, int bs)
            => RakNetService.HandleIncomingPacket(playerId, packetId, bs);

        [Callback]
        public void RNB_OnIncomingRPC(int playerId, int rpcId, int bs)
            => RakNetService.HandleIncomingRPC(playerId, rpcId, bs);

        [Callback]
        public void RNB_OnOutgoingPacket(int playerId, int packetId, int bs)
            => RakNetService.HandleOutgoingPacket(playerId, packetId, bs);

        [Callback]
        public void RNB_OnOutgoingRPC(int playerId, int rpcId, int bs)
            => RakNetService.HandleOutgoingRPC(playerId, rpcId, bs);
    }
}