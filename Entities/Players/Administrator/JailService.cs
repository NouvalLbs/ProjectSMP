using ProjectSMP.Extensions;
using ProjectSMP.Features.EnterExit;
using SampSharp.GameMode;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using SampSharp.GameMode.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSMP.Entities.Players.Administrator
{
    public static class JailService
    {
        private const int MaxJailTime = 3600;
        private static readonly Vector3 JailPosition = new(265.0534f, 77.6823f, 1001.0391f);
        private static readonly HashSet<int> JailedPlayers = new();
        private static readonly Dictionary<int, PlayerTextDraw> JailTextDraws = new();
        private static Timer UpdateTimer;

        public static void Initialize()
        {
            UpdateTimer = new Timer(1000, true);
            UpdateTimer.Tick += OnUpdate;
        }

        public static void Dispose()
        {
            UpdateTimer?.Dispose();
        }

        public static bool JailPlayer(Player target, int seconds, string reason)
        {
            if (seconds < 1 || seconds > MaxJailTime) return false;

            var pos = target.Position;
            target.SavedPosX = pos.X;
            target.SavedPosY = pos.Y;
            target.SavedPosZ = pos.Z;
            target.SavedInterior = target.Interior;
            target.SavedWorld = target.VirtualWorld;

            target.JailInfo.Jailed = 1;
            target.JailInfo.Time = seconds;
            target.JailInfo.Reason = reason;

            if (target.InAnyVehicle)
                target.RemoveFromVehicle();

            target.Cuffed = false;
            target.SetSpecialActionSafe(SpecialAction.None);

            target.SetInteriorSafe(6);
            target.SetVirtualWorldSafe(target.Id);
            target.SetPositionSafe(JailPosition);
            target.Angle = 270.0f;
            target.PutCameraBehindPlayer();

            target.ToggleControllableSafe(false);
            var timer = new Timer(2000, false);
            timer.Tick += (s, e) => { target.ToggleControllableSafe(true); timer.Dispose(); };

            CreateJailTextDraw(target);
            UpdateJailTextDraw(target, seconds);
            JailedPlayers.Add(target.Id);

            return true;
        }

        public static void UnjailPlayer(Player target)
        {
            target.JailInfo.Jailed = 0;
            target.JailInfo.Time = 0;
            target.JailInfo.Reason = "";

            if (JailTextDraws.TryGetValue(target.Id, out var td))
            {
                td.Hide();
                td.Dispose();
                JailTextDraws.Remove(target.Id);
            }

            target.SetInteriorSafe(target.SavedInterior);
            target.SetVirtualWorldSafe(target.SavedWorld);
            target.SetPositionSafe(target.SavedPosX, target.SavedPosY, target.SavedPosZ);
            target.PutCameraBehindPlayer();

            JailedPlayers.Remove(target.Id);
        }

        public static void OnPlayerSpawn(Player player)
        {
            if (player.JailInfo.Jailed > 0 && player.JailInfo.Time > 0)
            {
                player.SetInteriorSafe(6);
                player.SetVirtualWorldSafe(player.Id);
                player.SetPositionSafe(JailPosition);
                player.Angle = 270.0f;
                player.PutCameraBehindPlayer();

                CreateJailTextDraw(player);
                UpdateJailTextDraw(player, player.JailInfo.Time);

                player.SendClientMessage(Color.White, $"{{992712}}Kamu masih memiliki waktu jail tersisa sebanyak {player.JailInfo.Time} detik.");
                player.SendClientMessage(Color.White, $"{{992712}}Alasan: {player.JailInfo.Reason}");

                EnterExitService.ProcessEnterExit(player, () => {
                    if (!player.IsDisposed)
                        player.ToggleControllableSafe(true);
                });
            }
        }

        private static void OnUpdate(object sender, EventArgs e)
        {
            foreach (var player in BasePlayer.All.OfType<Player>())
            {
                if (!player.IsCharLoaded || player.JailInfo.Time <= 0) continue;

                player.JailInfo.Time--;

                if (player.JailInfo.Time <= 0)
                {
                    UnjailPlayer(player);
                    player.SendClientMessage(Color.White, "{992712}Waktu jail kamu telah berakhir, kamu telah dibebaskan.");
                }
                else
                {
                    UpdateJailTextDraw(player, player.JailInfo.Time);

                    var pos = player.Position;
                    if (player.Interior != 6 || player.VirtualWorld != player.Id || pos.DistanceTo(JailPosition) > 15.0f)
                    {
                        player.SetInteriorSafe(6);
                        player.SetVirtualWorldSafe(player.Id);
                        player.SetPositionSafe(JailPosition);
                        player.Angle = 270.0f;
                        player.SendClientMessage(Color.White, "{992712}Jangan coba-coba untuk keluar dari jail!");
                    }
                }
            }
        }

        private static void CreateJailTextDraw(Player player)
        {
            if (JailTextDraws.TryGetValue(player.Id, out var existing))
                existing.Dispose();

            var td = new PlayerTextDraw(player, new Vector2(320.0f, 380.0f), "Waktu Jail: ~r~0 ~w~detik")
            {
                Alignment = TextDrawAlignment.Center,
                BackColor = new Color(0, 0, 0, 170),
                Font = TextDrawFont.Slim,
                LetterSize = new Vector2(0.5f, 1.5f),
                ForeColor = Color.White,
                Outline = 1,
                Proportional = true,
                Shadow = 1,
                UseBox = true,
                BoxColor = new Color(0, 0, 0, 136),
                Width = 160.0f
            };

            JailTextDraws[player.Id] = td;
        }

        private static void UpdateJailTextDraw(Player player, int seconds)
        {
            if (!JailTextDraws.TryGetValue(player.Id, out var td)) return;
            td.Text = $"Waktu Jail: ~r~{seconds} ~w~detik";
            td.Show();
        }

        public static void Cleanup(Player player)
        {
            JailedPlayers.Remove(player.Id);
            if (JailTextDraws.TryGetValue(player.Id, out var td))
            {
                td.Dispose();
                JailTextDraws.Remove(player.Id);
            }
        }
    }
}