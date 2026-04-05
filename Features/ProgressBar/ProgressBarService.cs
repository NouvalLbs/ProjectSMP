using ProjectSMP.Features.ProgressBar.Data;
using SampSharp.GameMode.Definitions;
using SampSharp.GameMode.Display;
using SampSharp.GameMode.SAMP;
using System;
using System.Collections.Generic;

namespace ProjectSMP.Features.ProgressBar
{
    public static class ProgressBarService
    {
        private static Timer _updateTimer;
        private static readonly Dictionary<int, PlayerTextDraw[]> PlayerTextDraws = new();

        public static void Initialize()
        {
            _updateTimer = new Timer(50, true);
            _updateTimer.Tick += OnTimerTick;
        }

        public static void Dispose()
        {
            _updateTimer?.Dispose();
            foreach (var draws in PlayerTextDraws.Values)
            {
                foreach (var draw in draws)
                    draw?.Dispose();
            }
            PlayerTextDraws.Clear();
        }

        public static void CreateProgressBar(Player player)
        {
            var textDraws = new PlayerTextDraw[3];

            textDraws[0] = new PlayerTextDraw(player, new SampSharp.GameMode.Vector2(321f, 396f), "_");
            textDraws[0].Font = TextDrawFont.Normal;
            textDraws[0].LetterSize = new SampSharp.GameMode.Vector2(0.6f, 1.45f);
            textDraws[0].Width = 400f;
            textDraws[0].Height = 111.5f;
            textDraws[0].Outline = 1;
            textDraws[0].Shadow = 0;
            textDraws[0].Alignment = TextDrawAlignment.Center;
            textDraws[0].ForeColor = new Color(255, 255, 255, 255);
            textDraws[0].BackColor = new Color(255, 255, 255, 255);
            textDraws[0].BoxColor = new Color(0, 0, 0, 155);
            textDraws[0].UseBox = true;
            textDraws[0].Proportional = true;
            textDraws[0].Selectable = false;

            textDraws[1] = new PlayerTextDraw(player, new SampSharp.GameMode.Vector2(265f, 396f), "_");
            textDraws[1].Font = TextDrawFont.Normal;
            textDraws[1].LetterSize = new SampSharp.GameMode.Vector2(0.6f, 1.45f);
            textDraws[1].Width = 262f;
            textDraws[1].Height = 93.5f;
            textDraws[1].Outline = 1;
            textDraws[1].Shadow = 0;
            textDraws[1].Alignment = TextDrawAlignment.Left;
            textDraws[1].ForeColor = new Color(255, 255, 255, 255);
            textDraws[1].BackColor = new Color(255, 255, 255, 255);
            textDraws[1].BoxColor = new Color(28, 108, 200, 255);
            textDraws[1].UseBox = true;
            textDraws[1].Proportional = true;
            textDraws[1].Selectable = false;

            textDraws[2] = new PlayerTextDraw(player, new SampSharp.GameMode.Vector2(320f, 399f), "Loading...");
            textDraws[2].Font = TextDrawFont.Slim;
            textDraws[2].LetterSize = new SampSharp.GameMode.Vector2(0.15f, 0.7f);
            textDraws[2].Width = 400f;
            textDraws[2].Height = 17f;
            textDraws[2].Outline = 0;
            textDraws[2].Shadow = 0;
            textDraws[2].Alignment = TextDrawAlignment.Center;
            textDraws[2].ForeColor = new Color(255, 255, 255, 255);
            textDraws[2].BackColor = new Color(255, 255, 255, 255);
            textDraws[2].BoxColor = new Color(50, 50, 50, 50);
            textDraws[2].UseBox = false;
            textDraws[2].Proportional = true;
            textDraws[2].Selectable = false;

            PlayerTextDraws[player.Id] = textDraws;
        }

        public static void DestroyProgressBar(Player player)
        {
            if (!PlayerTextDraws.ContainsKey(player.Id)) return;

            foreach (var draw in PlayerTextDraws[player.Id])
            {
                draw?.Hide();
                draw?.Dispose();
            }

            PlayerTextDraws.Remove(player.Id);
            player.ProgressBarData.IsActive = false;
            player.ProgressBarData.Percentage = 0;
            player.ProgressBarData.CallbackType = ProgressCallbackType.NoCallback;
        }

        public static void StartProgress(Player player, int duration, string text,
            ProgressCallbackType callbackType = ProgressCallbackType.NoCallback,
            int animIndex = -1, string animLib = "", string animName = "",
            int itemSlot = -1, string itemName = "")
        {
            if (!player.IsConnected || !player.IsCharLoaded || player.ProgressBarData.IsActive)
                return;

            CreateProgressBar(player);

            player.ProgressBarData.IsActive = true;
            player.ProgressBarData.Duration = duration;
            player.ProgressBarData.Percentage = 0;
            player.ProgressBarData.CallbackType = callbackType;
            player.ProgressBarData.AnimIndex = animIndex;
            player.ProgressBarData.AnimLib = animLib;
            player.ProgressBarData.AnimName = animName;
            player.ProgressBarData.ItemSlot = itemSlot;
            player.ProgressBarData.ItemName = itemName;

            var textDraws = PlayerTextDraws[player.Id];
            textDraws[2].Text = text;

            foreach (var draw in textDraws)
                draw.Show();

            if (!string.IsNullOrEmpty(animLib) && !string.IsNullOrEmpty(animName))
            {
                var inVehicle = player.InAnyVehicle;
                player.ApplyAnimation(animLib, animName, 4.1f, !inVehicle, false, false, false, 0);
            }
        }

        private static void OnTimerTick(object sender, EventArgs e)
        {
            foreach (var player in SampSharp.GameMode.World.BasePlayer.All)
            {
                if (player is not Player p || !p.IsConnected || !p.IsCharLoaded) continue;
                if (!p.ProgressBarData.IsActive) continue;

                UpdateProgress(p);
            }
        }

        private static void UpdateProgress(Player player)
        {
            var data = player.ProgressBarData;
            var increment = 100f / (data.Duration * 20f);
            data.Percentage += increment;

            if (!string.IsNullOrEmpty(data.AnimLib) && !string.IsNullOrEmpty(data.AnimName))
            {
                var playerAnim = player.AnimationIndex;
                if (playerAnim != -1 && playerAnim != data.AnimIndex)
                {
                    var inVehicle = player.InAnyVehicle;
                    player.ApplyAnimation(data.AnimLib, data.AnimName, 4.1f, !inVehicle, false, false, false, 0);
                }
            }

            if (data.Percentage >= 100f)
            {
                data.Percentage = 100f;
                var inVehicle = player.InAnyVehicle;

                if (!string.IsNullOrEmpty(data.AnimLib) && !string.IsNullOrEmpty(data.AnimName) && !inVehicle)
                    player.ClearAnimations();

                ExecuteCallback(player);
                DestroyProgressBar(player);
                return;
            }

            if (!PlayerTextDraws.ContainsKey(player.Id)) return;

            var boxWidth = 262f + (115f * (data.Percentage / 100f));
            PlayerTextDraws[player.Id][1].Width = boxWidth;
            PlayerTextDraws[player.Id][1].Show();
        }

        private static void ExecuteCallback(Player player)
        {
            var data = player.ProgressBarData;

            switch (data.CallbackType)
            {
                case ProgressCallbackType.UseItem:
                    Entities.Players.Inventory.InventoryService.OnItemUseComplete(
                        player, data.ItemName, data.ItemSlot);
                    break;
            }
        }

        public static void OnPlayerDisconnect(Player player)
        {
            if (PlayerTextDraws.ContainsKey(player.Id))
                DestroyProgressBar(player);
        }
    }
}