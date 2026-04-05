using ProjectSMP.Core;
using ProjectSMP.Entities.Players.Needs;
using ProjectSMP.Extensions;
using ProjectSMP.Plugins.WeaponConfig;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System.Collections.Generic;

namespace ProjectSMP.Entities.Players.Condition
{
    public static class ConditionService
    {
        private static SampSharp.GameMode.SAMP.Timer _timer;
        private static readonly Dictionary<int, bool> _hospitalRespawn = new();

        public static void Initialize()
        {
            _timer = new SampSharp.GameMode.SAMP.Timer(1000, true);
            _timer.Tick += OnTimerTick;

            WeaponConfigService.PlayerPrepareDeath += OnPlayerPrepareDeath;
            WeaponConfigService.PlayerDeathFinished += OnPlayerDeathFinished;
        }

        public static void Dispose()
        {
            if (_timer != null && _timer.IsRunning)
                _timer.Dispose();

            _hospitalRespawn.Clear();

            WeaponConfigService.PlayerPrepareDeath -= OnPlayerPrepareDeath;
            WeaponConfigService.PlayerDeathFinished -= OnPlayerDeathFinished;
        }

        public static void RegisterPlayer(Player player)
        {
            _hospitalRespawn[player.Id] = false;
        }

        public static void UnregisterPlayer(Player player)
        {
            _hospitalRespawn.Remove(player.Id);
            DeathScreenManager.Destroy(player);
        }

        public static void HandleDeath(Player player)
        {
            if (player.Condition.Injured < 1)
            {
                player.SendClientMessage(Color.White, $"{Msg.Error} Kamu tidak terluka.");
                return;
            }

            if (player.Condition.DyingStage == 1)
            {
                player.SendClientMessage(Color.White, $"{Msg.Death} Tunggu hingga stage 2.");
                return;
            }

            _hospitalRespawn[player.Id] = true;
            WeaponConfigService.ForceRespawnFromDeath(player);
        }

        private static void OnTimerTick(object sender, System.EventArgs e)
        {
            foreach (var player in BasePlayer.All)
            {
                if (player is not Player p || !p.IsCharLoaded) continue;

                ProcessDeathHandler(p);
            }
        }

        private static void ProcessDeathHandler(Player player)
        {
            if (player.Condition.DyingTime > 0)
            {
                player.Condition.DyingTime--;
                if (player.Condition.DyingTime < 0)
                    player.Condition.DyingTime = 0;
            }

            var uptime = player.Condition.DyingTime;
            if (player.Condition.Injured == 1)
            {
                var minutes = uptime / 60;
                var seconds = uptime % 60;
                DeathScreenManager.UpdateTimer(player, minutes, seconds);

                if (player.Condition.DyingStage == 1 && uptime == 0)
                {
                    player.Condition.DyingStage = 2;
                    player.Condition.DyingTime = 900;
                    player.Condition.DeathAnimLib = "PED";
                    player.Condition.DeathAnimName = "FLOOR_HIT";
                    player.ApplyAnimationSafe("PED", "FLOOR_HIT", 4.0f, false, false, false, true, 0, true);
                    DeathScreenManager.UpdateStatus(player, 2);
                    player.SendClientMessage(Color.White, $"{Msg.Death} Kamu bisa respawn, ketik {{FFFF00}}/death{{ffffff}}");
                } else if (player.Condition.DyingStage == 2 && uptime == 0) {
                    _hospitalRespawn[player.Id] = true;
                    WeaponConfigService.ForceRespawnFromDeath(player);
                }
            }
        }

        private static void OnPlayerPrepareDeath(object sender, PrepareDeathArgs e)
        {
            if (e.Player.Condition.Injured == 0)
            {
                SaveDeathPosition(e.Player);
                e.Player.Condition.DyingStage = 1;
                e.Player.Condition.DyingTime = 3600;
                e.Player.Condition.DeathAnimLib = e.AnimLib;
                e.Player.Condition.DeathAnimName = e.AnimName;
            }

            e.Player.Condition.Injured = 1;
            NeedsHudManager.HideHud(e.Player);
            DeathScreenManager.Destroy(e.Player);
            DeathScreenManager.Create(e.Player);

            e.RespawnTime = int.MaxValue;
        }

        private static void OnPlayerDeathFinished(object sender, DeathFinishedArgs e)
        {
            if (_hospitalRespawn.TryGetValue(e.Player.Id, out var isHospital) && isHospital)
            {
                AfterDeathInit(e.Player);
            }
            else
            {
                AfterReviveInit(e.Player);
            }
        }

        private static void SaveDeathPosition(Player player)
        {
            var pos = player.Position;
            player.CharSpawnPos.X = pos.X;
            player.CharSpawnPos.Y = pos.Y;
            player.CharSpawnPos.Z = pos.Z;
            player.CharSpawnPos.A = player.Angle;
            player.CharSpawnPos.Interior = player.Interior;
            player.CharSpawnPos.World = player.GetVirtualWorldSafe();
        }

        private static void AfterDeathInit(Player player)
        {
            player.Condition.Injured = 0;
            player.Condition.DyingStage = 0;

            NeedsService.SetHunger(player, 50);
            NeedsService.SetEnergy(player, 50);

            player.SetHealthSafe(50, 0);

            for (var i = 0; i < 15; i++)
                player.SendClientMessage(Color.White, "");

            player.SendClientMessage(Color.White, $"{Msg.Death} Kamu telah menyerah dan menerima kematianmu.");
            player.SendClientMessage(Color.White, $"{Msg.Hospital} Kamu telah keluar dari rumah sakit, kamu membayar $10.50 kerumah sakit.");

            player.ToggleControllableSafe(true);
            player.SetVirtualWorldSafe(0);
            player.SetInteriorSafe(0);

            player.SetPositionSafe(1182.8778f, -1324.2023f, 13.5784f);
            player.Angle = 269.8747f;

            player.ClearAnimationsSafe();
            player.Condition.DyingTime = 0;

            NeedsHudManager.Initialize(player);
            DeathScreenManager.Destroy(player);
            _hospitalRespawn[player.Id] = false;
        }

        private static void AfterReviveInit(Player player)
        {
            player.Condition.Injured = 0;

            player.SetHealthSafe(100, 0);
            player.ToggleControllableSafe(true);

            player.SetVirtualWorldSafe(player.CharSpawnPos.World);
            player.SetInteriorSafe(player.CharSpawnPos.Interior);

            player.ClearAnimationsSafe();
            player.Condition.DyingTime = 0;

            NeedsHudManager.Initialize(player);
            DeathScreenManager.Destroy(player);
            _hospitalRespawn[player.Id] = false;
        }

        public static void RestoreDeathState(Player player)
        {
            if (player.Condition.Injured == 1 && player.Condition.DyingTime > 0)
            {
                var animLib = player.Condition.DyingStage == 1
                    ? player.Condition.DeathAnimLib
                    : "PED";
                var animName = player.Condition.DyingStage == 1
                    ? player.Condition.DeathAnimName
                    : "FLOOR_HIT";

                WeaponConfigService.ResumeDeath(player, animLib, animName);
                DeathScreenManager.UpdateStatus(player, player.Condition.DyingStage);
            }
        }

        public static void RevivePlayerInPlace(Player player)
        {
            if (player.Condition.Injured < 1) return;
            
            _hospitalRespawn[player.Id] = false;
            WeaponConfigService.ForceRespawnFromDeath(player);
        }
    }
}